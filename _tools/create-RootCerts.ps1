$RootCACert = New-SelfSignedCertificate -KeyUsage KeyEncipherment, DataEncipherment, CertSign `
                                         -HashAlgorithm SHA256 `
                                         -KeyUsageProperty All `
                                         -KeyLength 4096 `
                                         -Subject "IoTE2E-RootCA" `
                                         -FriendlyName "IoTE2E-RootCA"  `
                                         -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
                                         -certstorelocation cert:\CurrentUser\My  `
                                         -TextExtension @("2.5.29.19 ={text}ca=1&pathlength=3")


$SubCACert = New-SelfSignedCertificate -KeyUsage KeyEncipherment, DataEncipherment, CertSign `
                                        -HashAlgorithm SHA256 `
                                        -KeyUsageProperty All `
                                        -KeyLength 4096 `
                                        -Subject "IoTE2E-IntermediateCA" `
                                        -FriendlyName "IoTE2E-IntermediateCA"  `
                                        -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
                                        -certstorelocation cert:\CurrentUser\my  `
                                        -TextExtension @("2.5.29.19 ={text}ca=1&pathlength=0") `
                                        -Signer $RootCACert

$RootCACert
$SubCACert