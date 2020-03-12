$SignerCert = gci cert:\CurrentUser\My\18c79d6fc69781a8013dbf79e948ee5f4821376c
$VAlidationCert  = New-SelfSignedCertificate -KeyUsage KeyEncipherment, DataEncipherment, CertSign `
                                        -HashAlgorithm SHA256 `
                                        -KeyUsageProperty All `
                                        -KeyLength 4096 `
                                        -Subject "DBB5C26193F8630BEADB4F646AA24982FFC97AE06A902856"  `
                                        -FriendlyName "DBB5C26193F8630BEADB4F646AA24982FFC97AE06A902856" `
                                        -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
                                        -certstorelocation cert:\CurrentUser\my  `
                                        -Signer $SignerCert
$VAlidationCert