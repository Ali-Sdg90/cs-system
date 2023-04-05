﻿using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using CrystallineSociety.Shared.Dtos.BadgeSystem;
using Octokit;

namespace CrystallineSociety.Server.Api.Services.Implementations
{
    public partial class GitHubBadgeService : IGitHubBadgeService
    {
        [AutoInject] public IBadgeUtilService BadgeUtilService { get; set; }

        //todo:REMOVE   [AutoInject] public GitHubClient GitHubClient { get; set; }
        public GitHubClient GitHubClient { get; set; } = CreateClient();

        public async Task<List<BadgeDto>> GetBadgesAsync(string folderUrl)
        {
            var lightBadges = await GetLightBadgesAsync(folderUrl);

            var badges = new List<BadgeDto>();

            // ToDo: Ask about throwing an exception for any problematic item or just pass to the next item?
            foreach (var lightBadge in lightBadges)
            {
                if (lightBadge.Url is null || !lightBadge.Url.EndsWith("-badge.json"))
                    continue;

                var badgeDto = await GetBadgeAsync(
                    lightBadge.RepoId ?? throw new Exception("RepoId of light badge is null."),
                    lightBadge.Sha ?? throw new Exception("Sha of light badge is null.")
                );

                badges.Add(badgeDto);
            }

            return badges;
        }

        public async Task<List<BadgeDto>> GetLightBadgesAsync(string url)
        {
            var (orgName, repoName) = GetRepoAndOrgNameFromUrl(url);
            var repos = await GitHubClient.Repository.GetAllForOrg(orgName);
            var repo = repos.First(r => r.Name == repoName);

            var lastSegment = GetLastSegmentFromUrl(url, out var parentFolderPath);
            var repositoryId = repo.Id;

            // ToDo: Handle if the given url is a file url instead of a folder url which is required here.
            var folderContents = await GitHubClient.Repository.Content.GetAllContents(repositoryId, parentFolderPath);
            var folderSha = folderContents?.First(f => f.Name == lastSegment).Sha;
            var allContents = await GitHubClient.Git.Tree.GetRecursive(repositoryId, folderSha);

            return allContents.Tree
                .Select(t => new BadgeDto {RepoId = repositoryId, Sha = t.Sha, Url = t.Url})
                .ToList();
        }
        
        public async Task<BadgeDto> GetBadgeAsync(string badgeUrl)
        {
            var (orgName, repoName) = GetRepoAndOrgNameFromUrl(badgeUrl);
            var repo = await GitHubClient.Repository.Get(orgName, repoName);
            var refs = await GitHubClient.Git.Reference.GetAll(repo.Id);
            var branchName = GetBranchNameFromUrl(badgeUrl, refs) ??
                             throw new ResourceNotFoundException($"Unable to locate branchName: {badgeUrl}");
            
            var branchRef = refs.First(r => r.Ref.Contains($"refs/heads/{branchName}"));
            var badgeFilePath = GetRelativePath(badgeUrl.EndsWith("-badge.json")
                ? badgeUrl
                : throw new FileNotFoundException($"Badge file not found in: {badgeUrl}"));

            //ToDo: Check is there any method to get single file content?
            var contents =
                await GitHubClient.Repository.Content.GetAllContentsByRef(repo.Id, badgeFilePath, branchRef.Ref);
            var badgeFile = contents!.First();
            var badgeFileContent = badgeFile.Content;

            try
            {
                var badge = BadgeUtilService.ParseBadge(badgeFileContent);
                return badge;
            }
            catch (Exception exception)
            {
                throw new FormatException($"Can not parse badge with badgeUrl: '{badgeUrl}'", exception);
            }
        }
        
        public async Task<BadgeDto> GetBadgeAsync(long repositoryId, string sha)
        {
            var badgeBlob = await GitHubClient.Git.Blob.Get(repositoryId, sha);

            var bytes = Convert.FromBase64String(badgeBlob.Content);
            var badgeContent = Encoding.UTF8.GetString(bytes);
            var badgeDto = BadgeUtilService.ParseBadge(badgeContent);
            return badgeDto;
        }
        
        private static GitHubClient CreateClient()
        {
            return new GitHubClient(new ProductHeaderValue("CS-System"));
        }

        private static string GetRelativePath(string url)
        {
            // ToDo: Remove src dependence. 
            var urlSrcIndex = url.IndexOf("src", StringComparison.Ordinal);
            var folderPath = url[urlSrcIndex..];
            return folderPath;
        }

        private static string? GetBranchNameFromUrl(string url, IReadOnlyList<Reference> refs)
        {
            var uri = new Uri(url);
            var afterTreeSegments = String.Join("", uri.Segments[4..]);
            foreach (var reference in refs)
            {
                var branchInRefWithEndingSlash = $"{Regex.Replace(reference.Ref, @"^[^/]+/[^/]+/", "")}/";
                if (afterTreeSegments.StartsWith(branchInRefWithEndingSlash))
                {
                    return branchInRefWithEndingSlash.TrimEnd('/');
                }
            }

            return null;
        }

        private static string GetLastSegmentFromUrl(string url, out string parentFolderPath)
        {
            var uri = new Uri(url);
            var lastSegment = uri.Segments.Last().TrimEnd('/');
            var parentFolderUrl = uri.GetLeftPart(UriPartial.Authority) +
                                  string.Join("", uri.Segments.Take(uri.Segments.Length - 1));
            // ToDo: Remove src dependence. 
            var urlSrcIndex = parentFolderUrl.IndexOf("src", StringComparison.Ordinal);
            parentFolderPath = parentFolderUrl[urlSrcIndex..];

            return lastSegment;
        }

        private static (string org, string repo) GetRepoAndOrgNameFromUrl(string url)
        {
            var uri = new Uri(url);
            var segments = uri.Segments;
            var org = segments[1].TrimEnd('/');
            var repo = segments[2].TrimEnd('/');

            return (org, repo);
        }
    }
}