# log into Azure AD
$userName = "user1@tenant1.onMicrosoft.com"
$password = ""

$securePassword = ConvertTo-SecureString –String $password –AsPlainText -Force
$credential = New-Object –TypeName System.Management.Automation.PSCredential `
                         –ArgumentList $userName, $securePassword

$authResult = Connect-AzureAD # -Credential $credential

# authenticate with Azure AD
$authResult = Connect-AzureAD 

# get user account ID for logged in user
$user = Get-AzureADUser -ObjectId $authResult.Account.Id

# add variables for applicaiton name and redirect URI
$appDisplayName = "Power BI Public Client"
$redirectUri = "http://localhost/app1234"

$aadApplication = New-AzureADApplication `
                        -DisplayName $appDisplayName `
                        -PublicClient $true `
                        -AvailableToOtherTenants $false `
                        -ReplyUrls @($redirectUri)

# retrieve application Id
$appId = $aadApplication.AppId

# create service principal for application
$serviceServicePrincipal = New-AzureADServicePrincipal -AppId $appId

# assign current user as application owner
Add-AzureADApplicationOwner -ObjectId $aadApplication.ObjectId -RefObjectId $user.ObjectId

# generate text file that contains application Id and redirect URI
$outputFile = "$PSScriptRoot\PowerBiPublicClient.txt"
Out-File -FilePath $outputFile -InputObject "--- Power BI Public Client ---"
Out-File -FilePath $outputFile -Append -InputObject "application-id: $appId"
Out-File -FilePath $outputFile -Append -InputObject "redirect-uri: $replyUrl"
Notepad $outputFile