# User
Someone who can login to the system. Can be someone that is not involved in learning in the CS Internship program.
 
| Property | Type | Description |
---|---|---
DisplayName | String |  
Roles | Enum.Role[]  | Learner, Company, CompanyCreator, ContentHero 
Learner | Learner


 # Learner
A user that is a part of CS Internship program and is in the middle of growing journey.

Property | Type | Description
---|---|---
Code | String   
FirstName | String   
LastName | String   
Summary | String   
Status  | Enum.LearnerStatus | PendingInfo, Active, Inactive
IsActive | Boolean   
BirthCity | String   
LivingCity | String   
AnimalCharacter | String   
MobileNo | String   
TelegramUsername | String   
EmailMicrosoft | String   
EmailGoogle | String   
EmailCrystallineSociety | String   
TwitterUsername | String   
LinkedInUrl | String   
GitHubUsername | String   
StackOverflowUsername | String   
EthereumPubKey | String   
TonPubKey | String   
TagStr | String   
BadgeStr| String | coordinator:12,typer:56  
PrivilegeStr | String   
RolesStr | String   

# Badge
A badge is indication of a specific quality of a learner. For example if you participate in writing a document you may get a `document-creator` badge for each of the documents you write. Or if you participate in enough documentations you may get `documentation-guru` badge.
| Property | Type | Description |
---|---|---
Code | String
Title | String
Description | String | Having this badge indicates these qualities
Benefits | String | The benefits of having this badge
Prerequisites | String
PrerequisitesJsonSourceUrl | String | Each badge has a github file which will be synced one-way from it.
PrerequisitesJson | String
Level | Enum.BadgeLevel | Bronze, Silver, Gold
IsApprovalRequired | Boolean


# LearnerBadge
| Property | Type | Description |
---|---|---
Learner | Learner
Badge | Badge
AchieveDate | Date
AchieveType | Enum.AchieveType | Manual, Automatic
ApproverType | BadgeApproverType | Maual, Automatic
Approver | Learner?
Description | String | Why and how this badge achieved.
RefUrl | String | URL reference for why this badge is assigned. eg. A post on stackoverflow or an activity page

# Privilege
| Property | Type | Description |
---|---|---
Code | String
Title | String
Description | String
Power | String
Prerequisites | String
PrerequisitesJson | String
IsNeedApproval | Boolean


# LearnerPrivilege
| Property | Type | Description |
---|---|---
Learner | Learner
Privilege | Privilege
AchieveDate | Date
AchieveType | Enum.AchieveType | Manual, Automatic
ApproverType | BadgeApproverType | Maual, Automatic
Approver | Learner?

