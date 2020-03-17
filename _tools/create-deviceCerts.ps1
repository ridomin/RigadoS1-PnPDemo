$SignerCert = gci cert:\CurrentUser\My\18c79d6fc69781a8013dbf79e948ee5f4821376c
$name = "nodevice5"
$c  = New-SelfSignedCertificate -KeyUsage KeyEncipherment, DataEncipherment, CertSign `
                                        -HashAlgorithm SHA256 `
                                        -KeyUsageProperty All `
                                        -KeyLength 4096 `
                                        -Subject "$name"  `
                                        -FriendlyName "$name" `
                                        -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
                                        -certstorelocation cert:\CurrentUser\my  `
                                        -Signer $SignerCert
$c

Export-Certificate -Cert $c -FilePath "$name.cer" -Type CERT
certutil -encode "$name.cer" "$name.der"
bash -c "openssl x509 -in $name.der -out $name.cer.pem"

$mypwd = ConvertTo-SecureString -String "1234" -Force -AsPlainText
Export-PfxCertificate -Cert $c -FilePath  "$name.pfx" -Password $mypwd
bash -c "openssl pkcs12 -in $name.pfx -out $name.key.pem -nodes"