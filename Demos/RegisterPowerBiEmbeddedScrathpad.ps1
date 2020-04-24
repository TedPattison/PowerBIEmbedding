# log into Azure AD
$userName = "user1@tenant1.onMicrosoft.com"
$password = ""

$securePassword = ConvertTo-SecureString –String $password –AsPlainText -Force
$credential = New-Object –TypeName System.Management.Automation.PSCredential `
                         –ArgumentList $userName, $securePassword

$authResult = Connect-AzureAD # -Credential $credential


# display name for new public client app
$appDisplayName = "Power BI Embedded Scratchpad"

# get user account ID for logged in user
$user = Get-AzureADUser -ObjectId $authResult.Account.Id

# get tenant name of logged in user
$tenantName = $authResult.TenantDomain

# create Azure AD Application
$replyUrl = "http://localhost/app1234"

$aadApplication = New-AzureADApplication `
                        -DisplayName $appDisplayName `
                        -PublicClient $true `
                        -AvailableToOtherTenants $false `
                        -ReplyUrls @($replyUrl)

# create service principal for application
$appId = $aadApplication.AppId
$serviceServicePrincipal = New-AzureADServicePrincipal -AppId $appId

# assign current user as application owner
Add-AzureADApplicationOwner -ObjectId $aadApplication.ObjectId -RefObjectId $user.ObjectId

# add permissions by updating application with RequiredResourceAccess object
Set-AzureADApplication -ObjectId $aadApplication.ObjectId -RequiredResourceAccess $requiredAccess

$outputFile = "$PSScriptRoot\PowerBiEmbeddedScratchpad.txt"
Out-File -FilePath $outputFile -InputObject "--- Power BI Embedded Scratchpad ---"
Out-File -FilePath $outputFile -Append -InputObject "application-id: $appId"
Out-File -FilePath $outputFile -Append -InputObject "redirect-uri: $replyUrl"

Notepad $outputFile