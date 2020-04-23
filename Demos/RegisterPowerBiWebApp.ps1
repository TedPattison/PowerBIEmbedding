# log into Azure AD
$userName = "user1@tenant1.onMicrosoft.com"
$password = ""

$securePassword = ConvertTo-SecureString –String $password –AsPlainText -Force
$credential = New-Object –TypeName System.Management.Automation.PSCredential `
                         –ArgumentList $userName, $securePassword

$authResult = Connect-AzureAD # -Credential $credential

$tenantId = $authResult.TenantId.ToString()
$tenantDomain = $authResult.TenantDomain
$tenantDisplayName = (Get-AzureADTenantDetail).DisplayName

$userAccountId = $authResult.Account.Id
$user = Get-AzureADUser -ObjectId $userAccountId
$userDisplayName = $user.DisplayName

$appDisplayName = "Power BI Web App"
$replyUrl = "https://localhost:44300"

# create app secret
$newGuid = New-Guid
$appSecret = ([System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes(($newGuid))))+"="
$startDate = Get-Date	
$passwordCredential = New-Object -TypeName Microsoft.Open.AzureAD.Model.PasswordCredential
$passwordCredential.StartDate = $startDate
$passwordCredential.EndDate = $startDate.AddYears(1)
$passwordCredential.KeyId = $newGuid
$passwordCredential.Value = $appSecret 

# create Azure AD Application
$aadApplication = New-AzureADApplication `
                        -DisplayName $appDisplayName `
                        -PublicClient $false `
                        -AvailableToOtherTenants $false `
                        -ReplyUrls @($replyUrl) `
                        -Homepage $replyUrl `
                        -PasswordCredentials $passwordCredential

# create applicaiton's service principal 
$appId = $aadApplication.AppId
$appObjectId = $aadApplication.ObjectId
$serviceServicePrincipal = New-AzureADServicePrincipal -AppId $appId

$outputFile = "$PSScriptRoot\PowerBiWebApp.txt"
Write-Host "Writing info to $outputFile"
Out-File -FilePath $outputFile -InputObject "--- Info for $appDisplayName ---"
Out-File -FilePath $outputFile -Append -InputObject "<add key='application-id' value='$appId' />"
Out-File -FilePath $outputFile -Append -InputObject "<add key='application-secret' value='$appSecret' />"
Out-File -FilePath $outputFile -Append -InputObject "<add key='redirect-uri' value='$replyUrl' />"

Notepad $outputFile