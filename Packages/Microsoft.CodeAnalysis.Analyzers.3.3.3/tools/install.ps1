﻿param($installPath, $toolsPath, $package, $project)

$analyzersDir = Join-Path (Split-Path -Path $toolsPath -Parent) "analyzers"
if (-Not (Test-Path $analyzersDir))
{
    return
}

$analyzersPaths = Join-Path ( $analyzersDir ) * -Resolve

foreach($analyzersPath in $analyzersPaths)
{
    # Install the language agnostic analyzers.
    if (Test-Path $analyzersPath)
    {
        foreach ($analyzerFilePath in Get-ChildItem $analyzersPath -Filter *.dll)
        {
            if($project.Object.AnalyzerReferences)
            {
                $project.Object.AnalyzerReferences.Add($analyzerFilePath.FullName)
            }
        }
    }
}

# $project.Type gives the language name like (C# or VB.NET)
$languageFolder = ""
if($project.Type -eq "C#")
{
    $languageFolder = "cs"
}
if($project.Type -eq "VB.NET")
{
    $languageFolder = "vb"
}
if($languageFolder -eq "")
{
    return
}

foreach($analyzersPath in $analyzersPaths)
{
    # Install language specific analyzers.
    $languageAnalyzersPath = join-path $analyzersPath $languageFolder
    if (Test-Path $languageAnalyzersPath)
    {
        foreach ($analyzerFilePath in Get-ChildItem $languageAnalyzersPath -Filter *.dll)
        {
            if($project.Object.AnalyzerReferences)
            {
                $project.Object.AnalyzerReferences.Add($analyzerFilePath.FullName)
            }
        }
    }
}
# SIG # Begin signature block
# MIIkdQYJKoZIhvcNAQcCoIIkZjCCJGICAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCCh1voBODYpfCet
# zwmmor0XvG3ngI9/l8E/7NCzPBjyWaCCDfAwggZuMIIEVqADAgECAhMzAAACE4wM
# HDE1vNJfAAAAAAITMA0GCSqGSIb3DQEBDAUAMH4xCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25p
# bmcgUENBIDIwMTEwHhcNMjEwMjExMjAwOTUxWhcNMjIwMjEwMjAwOTUxWjBjMQsw
# CQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9u
# ZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMQ0wCwYDVQQDEwQuTkVU
# MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAm1lwQt2sYeUcIXqfPhsf
# y9aXkzcZ9ch/sljs6i9HCDB5Om0lAZvLRgdn6IKtYgTt9Nz9z14Yoab4Sy5Yedqc
# Cb1GjsxKN0+xgFFqJ/Vjp3KEFxtvjiC5Q0qWmWrsvWTeV1cUXPIC367xTMIvlAXC
# JuDadaJCYKSglaGIDpHEQ+ARn+yzYgKmmAsebWjq/LU/hyLpoyROPazW5y3+gV+V
# jATQOwrU6sUftC+gipYIC5dReNk4qcWhGJrbb5yLajGz8tWSCpwzALfkoe9QHMJb
# cswT91IFYI5NXuCJ4b6BS3EGA+nOiZqTi2G2tAsxEBaXsA60jtC+xdoUTJCkCUNX
# PXZJrz0ok4sE+SwFEPDjChFkuUedSu3EVGkxmyv71x/bN9k8biJraINIEV9e7YF5
# euJ+RPhz9RXkjLeoQcLOHWMuY3vTWNQ6EXVA3UsQ+f5MK2HZvbf3/fj4HKZwgBwM
# /URaTwmh2xQN50qEr6UB580vSbNSshBzoPFvlyqZrL9BAgMBAAGjggF+MIIBejAf
# BgNVHSUEGDAWBgorBgEEAYI3TAgBBggrBgEFBQcDAzAdBgNVHQ4EFgQU701pILfc
# jCxDQ4qlYedyWy17rTkwUAYDVR0RBEkwR6RFMEMxKTAnBgNVBAsTIE1pY3Jvc29m
# dCBPcGVyYXRpb25zIFB1ZXJ0byBSaWNvMRYwFAYDVQQFEw00NjQyMjMrNDY0Mjkz
# MB8GA1UdIwQYMBaAFEhuZOVQBdOCqhc3NyK1bajKdQKVMFQGA1UdHwRNMEswSaBH
# oEWGQ2h0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvY3JsL01pY0NvZFNp
# Z1BDQTIwMTFfMjAxMS0wNy0wOC5jcmwwYQYIKwYBBQUHAQEEVTBTMFEGCCsGAQUF
# BzAChkVodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpb3BzL2NlcnRzL01pY0Nv
# ZFNpZ1BDQTIwMTFfMjAxMS0wNy0wOC5jcnQwDAYDVR0TAQH/BAIwADANBgkqhkiG
# 9w0BAQwFAAOCAgEAWIP5xHQrp68xR5SsEwCkuWNXrkYMNPOUPiOqvoy7U1N5m/kv
# 0gmVHoMsAQpomcFrt9ln8wT8VlqyZzjT5aKOEOa2H845zsil8ndFrVq4ogCDrzua
# j95aI+zScLjagJ1yHL1DkBLsrT4xSTtH+n14JM81gWNIwi2Iv5gVgLFpleoeLPB1
# dy8IisRKrgNfA35l1R4YEQ70XZKQxULrX/LbHBLBFaZ87+Xhi60pP51IqOV6ViLM
# UBBomsbM7lpznpL4r80yd08vs53eiGrEy7uCwVTKfFiX57IH1P0j0etRagePpx7i
# fM7EcL7INEPoyLYBpP4hsw5wa9Z4kLmJfe3Okyp3PQE4yXqNt9/634EtLtW1TokA
# UIx+TmH7Cog9aOife2lO4W0KQw5LMiiyjdTqR1fR+CuBec0omykFeR8ob2Uw8484
# rDO5rWqOCG+DM8x+2PXJmoWWMmZyMHGz8coDwnyy3UwpyCIEYNfnORywdga4CQhv
# Z9dC5hbLQbvwnPNK/W7QsAWPz9C3H0ebbB9zbT7DD4xgRD227UW4lkrY7v6nuJXO
# LBm+j5dXO4hTuxWNyYkfkaf+21+QtDfSO8z1DJX+VjC6uDgafkuL8hlI3npYYg+K
# VGRdRW6ulpVZ5gKN5QnMxjafa1UwWXqmCcaZoDZrS6IbfPQakw91tLVH+gQwggd6
# MIIFYqADAgECAgphDpDSAAAAAAADMA0GCSqGSIb3DQEBCwUAMIGIMQswCQYDVQQG
# EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwG
# A1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMTIwMAYDVQQDEylNaWNyb3NvZnQg
# Um9vdCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkgMjAxMTAeFw0xMTA3MDgyMDU5MDla
# Fw0yNjA3MDgyMTA5MDlaMH4xCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5n
# dG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9y
# YXRpb24xKDAmBgNVBAMTH01pY3Jvc29mdCBDb2RlIFNpZ25pbmcgUENBIDIwMTEw
# ggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQCr8PpyEBwurdhuqoIQTTS6
# 8rZYIZ9CGypr6VpQqrgGOBoESbp/wwwe3TdrxhLYC/A4wpkGsMg51QEUMULTiQ15
# ZId+lGAkbK+eSZzpaF7S35tTsgosw6/ZqSuuegmv15ZZymAaBelmdugyUiYSL+er
# CFDPs0S3XdjELgN1q2jzy23zOlyhFvRGuuA4ZKxuZDV4pqBjDy3TQJP4494HDdVc
# eaVJKecNvqATd76UPe/74ytaEB9NViiienLgEjq3SV7Y7e1DkYPZe7J7hhvZPrGM
# XeiJT4Qa8qEvWeSQOy2uM1jFtz7+MtOzAz2xsq+SOH7SnYAs9U5WkSE1JcM5bmR/
# U7qcD60ZI4TL9LoDho33X/DQUr+MlIe8wCF0JV8YKLbMJyg4JZg5SjbPfLGSrhwj
# p6lm7GEfauEoSZ1fiOIlXdMhSz5SxLVXPyQD8NF6Wy/VI+NwXQ9RRnez+ADhvKwC
# gl/bwBWzvRvUVUvnOaEP6SNJvBi4RHxF5MHDcnrgcuck379GmcXvwhxX24ON7E1J
# MKerjt/sW5+v/N2wZuLBl4F77dbtS+dJKacTKKanfWeA5opieF+yL4TXV5xcv3co
# KPHtbcMojyyPQDdPweGFRInECUzF1KVDL3SV9274eCBYLBNdYJWaPk8zhNqwiBfe
# nk70lrC8RqBsmNLg1oiMCwIDAQABo4IB7TCCAekwEAYJKwYBBAGCNxUBBAMCAQAw
# HQYDVR0OBBYEFEhuZOVQBdOCqhc3NyK1bajKdQKVMBkGCSsGAQQBgjcUAgQMHgoA
# UwB1AGIAQwBBMAsGA1UdDwQEAwIBhjAPBgNVHRMBAf8EBTADAQH/MB8GA1UdIwQY
# MBaAFHItOgIxkEO5FAVO4eqnxzHRI4k0MFoGA1UdHwRTMFEwT6BNoEuGSWh0dHA6
# Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3Byb2R1Y3RzL01pY1Jvb0NlckF1
# dDIwMTFfMjAxMV8wM18yMi5jcmwwXgYIKwYBBQUHAQEEUjBQME4GCCsGAQUFBzAC
# hkJodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY1Jvb0NlckF1
# dDIwMTFfMjAxMV8wM18yMi5jcnQwgZ8GA1UdIASBlzCBlDCBkQYJKwYBBAGCNy4D
# MIGDMD8GCCsGAQUFBwIBFjNodHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpb3Bz
# L2RvY3MvcHJpbWFyeWNwcy5odG0wQAYIKwYBBQUHAgIwNB4yIB0ATABlAGcAYQBs
# AF8AcABvAGwAaQBjAHkAXwBzAHQAYQB0AGUAbQBlAG4AdAAuIB0wDQYJKoZIhvcN
# AQELBQADggIBAGfyhqWY4FR5Gi7T2HRnIpsLlhHhY5KZQpZ90nkMkMFlXy4sPvjD
# ctFtg/6+P+gKyju/R6mj82nbY78iNaWXXWWEkH2LRlBV2AySfNIaSxzzPEKLUtCw
# /WvjPgcuKZvmPRul1LUdd5Q54ulkyUQ9eHoj8xN9ppB0g430yyYCRirCihC7pKkF
# DJvtaPpoLpWgKj8qa1hJYx8JaW5amJbkg/TAj/NGK978O9C9Ne9uJa7lryft0N3z
# Dq+ZKJeYTQ49C/IIidYfwzIY4vDFLc5bnrRJOQrGCsLGra7lstnbFYhRRVg4MnEn
# Gn+x9Cf43iw6IGmYslmJaG5vp7d0w0AFBqYBKig+gj8TTWYLwLNN9eGPfxxvFX1F
# p3blQCplo8NdUmKGwx1jNpeG39rz+PIWoZon4c2ll9DuXWNB41sHnIc+BncG0Qax
# dR8UvmFhtfDcxhsEvt9Bxw4o7t5lL+yX9qFcltgA1qFGvVnzl6UJS0gQmYAf0AAp
# xbGbpT9Fdx41xtKiop96eiL6SJUfq/tHI4D1nvi/a7dLl+LrdXga7Oo3mXkYS//W
# syNodeav+vyL6wuA6mk7r/ww7QRMjt/fdW1jkT3RnVZOT7+AVyKheBEyIXrvQQqx
# P/uozKRdwaGIm1dxVk5IRcBCyZt2WwqASGv9eZ/BvW1taslScxMNelDNMYIV2zCC
# FdcCAQEwgZUwfjELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAO
# BgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEo
# MCYGA1UEAxMfTWljcm9zb2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAxMQITMwAAAhOM
# DBwxNbzSXwAAAAACEzANBglghkgBZQMEAgEFAKCBrjAZBgkqhkiG9w0BCQMxDAYK
# KwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAvBgkqhkiG
# 9w0BCQQxIgQgscbk7KKN8M4zlvGIkWbJVqzLt3vnYcsDip+8ebtDMX0wQgYKKwYB
# BAGCNwIBDDE0MDKgFIASAE0AaQBjAHIAbwBzAG8AZgB0oRqAGGh0dHA6Ly93d3cu
# bWljcm9zb2Z0LmNvbTANBgkqhkiG9w0BAQEFAASCAYBWyq9M/lWh9CwuErNcz6q4
# Zzyas+YrmvVSqpGlXaZDe673NcuLPSBKcDhYWxjV2DdiS3IVolRUq9UrCId3sO9t
# +hIKAMK/pH7m9xRlYzaULQs/F1F4D183kM6URYRu4/0L/p8I+w6IgoVxWH6MHJEf
# IaFME2sz6cvm7pOathObz0NyMiWmm2swSkOexPg9iuknnqkJzk5eFJQ9+P3oDcob
# p/Z2dsNd6h8e9IwPhnsL78Guv0BNadeVnF3LhcUJJSmQVsiwsREB98J1WiiMpE6f
# Bb83X9bQXOHRTktYj4z2CIEARom/lco08mvdTm1weayYNnkZR7tUWP/sBG3P4SzD
# yL+NXg6T3N2Vsl2eohnTngQvtOK3ZQg6SAHvclSCAVzI1ji6XcpX2AC5MURJzGQq
# vy+iJtneblQROETxTwk7+UJ9BZwtF+NQ0PlgK/N5HiXgmKMCOiCOxViHiitN3TyI
# vduAFsel2SEQzPAx4wp6Y9rQJW3PQJSxAbln5Ia2XZqhghLlMIIS4QYKKwYBBAGC
# NwMDATGCEtEwghLNBgkqhkiG9w0BBwKgghK+MIISugIBAzEPMA0GCWCGSAFlAwQC
# AQUAMIIBUQYLKoZIhvcNAQkQAQSgggFABIIBPDCCATgCAQEGCisGAQQBhFkKAwEw
# MTANBglghkgBZQMEAgEFAAQgRYJGxlSbI6pFkfAKB9/Y2ms3uKA12PwQaKVrfMkx
# 8KMCBmFtzs62hRgTMjAyMTEwMjEwNTUzMjcuNTA0WjAEgAIB9KCB0KSBzTCByjEL
# MAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1v
# bmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjElMCMGA1UECxMcTWlj
# cm9zb2Z0IEFtZXJpY2EgT3BlcmF0aW9uczEmMCQGA1UECxMdVGhhbGVzIFRTUyBF
# U046N0JGMS1FM0VBLUI4MDgxJTAjBgNVBAMTHE1pY3Jvc29mdCBUaW1lLVN0YW1w
# IFNlcnZpY2Wggg48MIIE8TCCA9mgAwIBAgITMwAAAVHDUOdZbKrGpwAAAAABUTAN
# BgkqhkiG9w0BAQsFADB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3Rv
# bjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0
# aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMDAeFw0y
# MDExMTIxODI2MDRaFw0yMjAyMTExODI2MDRaMIHKMQswCQYDVQQGEwJVUzETMBEG
# A1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWlj
# cm9zb2Z0IENvcnBvcmF0aW9uMSUwIwYDVQQLExxNaWNyb3NvZnQgQW1lcmljYSBP
# cGVyYXRpb25zMSYwJAYDVQQLEx1UaGFsZXMgVFNTIEVTTjo3QkYxLUUzRUEtQjgw
# ODElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2VydmljZTCCASIwDQYJ
# KoZIhvcNAQEBBQADggEPADCCAQoCggEBAJ/Sh++qhK477ziJI1mx6bTJGA45hviR
# Js4Lsq/1cY2YGf4oPDJOO46kiT+UcR/7A8qoWLu4z0jvOrImYfLuwwV/S/CPgAfv
# Hzz7w+LqCyg9tgaaBZeAfBcOSu0rom728Rje2nS9f81vrFl5Vb6Q4RDyCgyArxHT
# Yxky4ZLX37Y3n4PZbpgTFASdhuP4OGndHQ70TZiojGV13vy5eEIP6D0s1wlBGKEk
# qmuQ/uTEYplXuf2Ey49I1a/IheOVdIU+1R/DiTuGCJnJ2Yaug8NRvsOgAkRnjxZj
# lqlvLRGdd0jJjqria05MMsvM8jbVbbSQF+3YhS20dErzJWyWVitCh3cCAwEAAaOC
# ARswggEXMB0GA1UdDgQWBBTFd//jaFBikzRoOjjMhOnzdUTqbTAfBgNVHSMEGDAW
# gBTVYzpcijGQ80N7fEYbxTNoWoVtVTBWBgNVHR8ETzBNMEugSaBHhkVodHRwOi8v
# Y3JsLm1pY3Jvc29mdC5jb20vcGtpL2NybC9wcm9kdWN0cy9NaWNUaW1TdGFQQ0Ff
# MjAxMC0wNy0wMS5jcmwwWgYIKwYBBQUHAQEETjBMMEoGCCsGAQUFBzAChj5odHRw
# Oi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRzL01pY1RpbVN0YVBDQV8yMDEw
# LTA3LTAxLmNydDAMBgNVHRMBAf8EAjAAMBMGA1UdJQQMMAoGCCsGAQUFBwMIMA0G
# CSqGSIb3DQEBCwUAA4IBAQAr/fXAFYOZ8dEqo7y30M5roDI+XCfTROtHbkh9S6cR
# 2IpvS7N1H4mHe7dCb8hMP60UxCh2851eixS5V/vpRyTBis2Zx7U3tjiOmRxZzYhY
# bYMlrmAya5uykMpDYtRtS27lYnvTHoZqCvoQYmZ563H2UpwUqJK7ztkBFhwtcZ2e
# cDPNlBI6axWDpHIVPukXKAo45iBRn4EszY9TCG3+JXCeRaFdTIOhcBeOQoozlx1V
# 685IrDGfabg6RY4xFekwGOiDYDJIS3r/wFaMNLBfDH0M7SSJRWHRRJGeTRfyMs6A
# tmG/YsOGwinQa3Q9wLOpr6BkjYwgupTnc+hHqyStzYRYMIIGcTCCBFmgAwIBAgIK
# YQmBKgAAAAAAAjANBgkqhkiG9w0BAQsFADCBiDELMAkGA1UEBhMCVVMxEzARBgNV
# BAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jv
# c29mdCBDb3Jwb3JhdGlvbjEyMDAGA1UEAxMpTWljcm9zb2Z0IFJvb3QgQ2VydGlm
# aWNhdGUgQXV0aG9yaXR5IDIwMTAwHhcNMTAwNzAxMjEzNjU1WhcNMjUwNzAxMjE0
# NjU1WjB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UE
# BxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYwJAYD
# VQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0EgMjAxMDCCASIwDQYJKoZIhvcN
# AQEBBQADggEPADCCAQoCggEBAKkdDbx3EYo6IOz8E5f1+n9plGt0VBDVpQoAgoX7
# 7XxoSyxfxcPlYcJ2tz5mK1vwFVMnBDEfQRsalR3OCROOfGEwWbEwRA/xYIiEVEMM
# 1024OAizQt2TrNZzMFcmgqNFDdDq9UeBzb8kYDJYYEbyWEeGMoQedGFnkV+BVLHP
# k0ySwcSmXdFhE24oxhr5hoC732H8RsEnHSRnEnIaIYqvS2SJUGKxXf13Hz3wV3Ws
# vYpCTUBR0Q+cBj5nf/VmwAOWRH7v0Ev9buWayrGo8noqCjHw2k4GkbaICDXoeByw
# 6ZnNPOcvRLqn9NxkvaQBwSAJk3jN/LzAyURdXhacAQVPIk0CAwEAAaOCAeYwggHi
# MBAGCSsGAQQBgjcVAQQDAgEAMB0GA1UdDgQWBBTVYzpcijGQ80N7fEYbxTNoWoVt
# VTAZBgkrBgEEAYI3FAIEDB4KAFMAdQBiAEMAQTALBgNVHQ8EBAMCAYYwDwYDVR0T
# AQH/BAUwAwEB/zAfBgNVHSMEGDAWgBTV9lbLj+iiXGJo0T2UkFvXzpoYxDBWBgNV
# HR8ETzBNMEugSaBHhkVodHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtpL2NybC9w
# cm9kdWN0cy9NaWNSb29DZXJBdXRfMjAxMC0wNi0yMy5jcmwwWgYIKwYBBQUHAQEE
# TjBMMEoGCCsGAQUFBzAChj5odHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2Nl
# cnRzL01pY1Jvb0NlckF1dF8yMDEwLTA2LTIzLmNydDCBoAYDVR0gAQH/BIGVMIGS
# MIGPBgkrBgEEAYI3LgMwgYEwPQYIKwYBBQUHAgEWMWh0dHA6Ly93d3cubWljcm9z
# b2Z0LmNvbS9QS0kvZG9jcy9DUFMvZGVmYXVsdC5odG0wQAYIKwYBBQUHAgIwNB4y
# IB0ATABlAGcAYQBsAF8AUABvAGwAaQBjAHkAXwBTAHQAYQB0AGUAbQBlAG4AdAAu
# IB0wDQYJKoZIhvcNAQELBQADggIBAAfmiFEN4sbgmD+BcQM9naOhIW+z66bM9TG+
# zwXiqf76V20ZMLPCxWbJat/15/B4vceoniXj+bzta1RXCCtRgkQS+7lTjMz0YBKK
# dsxAQEGb3FwX/1z5Xhc1mCRWS3TvQhDIr79/xn/yN31aPxzymXlKkVIArzgPF/Uv
# eYFl2am1a+THzvbKegBvSzBEJCI8z+0DpZaPWSm8tv0E4XCfMkon/VWvL/625Y4z
# u2JfmttXQOnxzplmkIz/amJ/3cVKC5Em4jnsGUpxY517IW3DnKOiPPp/fZZqkHim
# bdLhnPkd/DjYlPTGpQqWhqS9nhquBEKDuLWAmyI4ILUl5WTs9/S/fmNZJQ96LjlX
# dqJxqgaKD4kWumGnEcua2A5HmoDF0M2n0O99g/DhO3EJ3110mCIIYdqwUB5vvfHh
# AN/nMQekkzr3ZUd46PioSKv33nJ+YWtvd6mBy6cJrDm77MbL2IK0cs0d9LiFAR6A
# +xuJKlQ5slvayA1VmXqHczsI5pgt6o3gMy4SKfXAL1QnIffIrE7aKLixqduWsqdC
# osnPGUFN4Ib5KpqjEWYw07t0MkvfY3v1mYovG8chr1m1rtxEPJdQcdeh0sVV42ne
# V8HR3jDA/czmTfsNv11P6Z0eGTgvvM9YBS7vDaBQNdrvCScc1bN+NR4Iuto229Nf
# j950iEkSoYICzjCCAjcCAQEwgfihgdCkgc0wgcoxCzAJBgNVBAYTAlVTMRMwEQYD
# VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
# b3NvZnQgQ29ycG9yYXRpb24xJTAjBgNVBAsTHE1pY3Jvc29mdCBBbWVyaWNhIE9w
# ZXJhdGlvbnMxJjAkBgNVBAsTHVRoYWxlcyBUU1MgRVNOOjdCRjEtRTNFQS1CODA4
# MSUwIwYDVQQDExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2aWNloiMKAQEwBwYF
# Kw4DAhoDFQCgoq9z8T+kQgslTCUgFaDFetcjXqCBgzCBgKR+MHwxCzAJBgNVBAYT
# AlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYD
# VQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJjAkBgNVBAMTHU1pY3Jvc29mdCBU
# aW1lLVN0YW1wIFBDQSAyMDEwMA0GCSqGSIb3DQEBBQUAAgUA5RrwQzAiGA8yMDIx
# MTAyMTAzNDUwN1oYDzIwMjExMDIyMDM0NTA3WjB3MD0GCisGAQQBhFkKBAExLzAt
# MAoCBQDlGvBDAgEAMAoCAQACAgNOAgH/MAcCAQACAhFsMAoCBQDlHEHDAgEAMDYG
# CisGAQQBhFkKBAIxKDAmMAwGCisGAQQBhFkKAwKgCjAIAgEAAgMHoSChCjAIAgEA
# AgMBhqAwDQYJKoZIhvcNAQEFBQADgYEAVBMCsTY5DrM+Ll81UWOOUd5v7IUkpakb
# gzO8jD5qz+XFIFoFNdzRux/63m1Use0L4/b8TeqQsRnEhrJlxpLUmhxzz6lJ/3Ig
# qxun0f0tiBrxwVdntnCMcdU1Z6mt85UEpX4y65OevVhX2wK1SxUhWGlRXvgsGLti
# PGmQrTAXS0cxggMNMIIDCQIBATCBkzB8MQswCQYDVQQGEwJVUzETMBEGA1UECBMK
# V2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0
# IENvcnBvcmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0Eg
# MjAxMAITMwAAAVHDUOdZbKrGpwAAAAABUTANBglghkgBZQMEAgEFAKCCAUowGgYJ
# KoZIhvcNAQkDMQ0GCyqGSIb3DQEJEAEEMC8GCSqGSIb3DQEJBDEiBCB/6PD1Abb8
# klSZd07tN9APchAnseJNhKHgeQSmLlDkRTCB+gYLKoZIhvcNAQkQAi8xgeowgecw
# geQwgb0EIC7NXJmI+NbBWQcAphb7/UnD+bbrlIcbL/7dAfVxeuVBMIGYMIGApH4w
# fDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1Jl
# ZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMd
# TWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTACEzMAAAFRw1DnWWyqxqcAAAAA
# AVEwIgQgd2MVPQGTdcyS6nIxLz3vuIN//F7lw+1me2y1JdvgX6EwDQYJKoZIhvcN
# AQELBQAEggEAeOg3K+KamZ2Oc1U8mo8552+HokO0IHIiPj00M/IelG+WqZlux1Ay
# /qfl+lM5Q/5duR93SnJ249HK7o8NK+xkTP7mVfnxSpgG0HfZ89KuF7szWuDzvv6L
# h3VZxFfa2x1HLRcLjIUr5vTydI/O2MM6fVPUHn8qSQUA9YK41rAKBOKy5LPBWNTw
# 55Iira4zL23noTDV9DVIB5+rMNhnV0m2soDvIEfJYZHD59d9eEeQwkAZr3pmTkCu
# dh7OuSx/cLFeh5qUa/A0a+qsmCBefIkmX1GLQqDaHUif2qoAwzrwDcCJc5jE/2Uk
# UJemb2V2RVk7Qd4WqDKs4EuwszVP25N0AA==
# SIG # End signature block
