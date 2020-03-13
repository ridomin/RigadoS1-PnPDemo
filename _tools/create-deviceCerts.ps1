$SignerCert = gci cert:\CurrentUser\My\18c79d6fc69781a8013dbf79e948ee5f4821376c
$DeviceCert  = New-SelfSignedCertificate -KeyUsage KeyEncipherment, DataEncipherment, CertSign `
                                        -HashAlgorithm SHA256 `
                                        -KeyUsageProperty All `
                                        -KeyLength 4096 `
                                        -Subject "Rigado.S1-Central-GA"  `
                                        -FriendlyName "Rigado.S1-Central-GA" `
                                        -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
                                        -certstorelocation cert:\CurrentUser\my  `
                                        -Signer $SignerCert
$DeviceCert