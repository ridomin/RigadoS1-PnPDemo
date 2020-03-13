New-SelfSignedCertificate -KeyUsage KeyEncipherment, DataEncipherment, CertSign `
                                        -HashAlgorithm SHA256 `
                                        -KeyUsageProperty All `
                                        -KeyLength 4096 `
                                        -Subject "mycert"  `
                                        -FriendlyName "mycert" `
                                        -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
                                        -certstorelocation cert:\CurrentUser\my  `
                                        