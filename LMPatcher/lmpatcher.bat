zipalign -f -v 4 original.apk aligned.apk
"C:\Program Files (x86)\Android\android-sdk\build-tools\25.0.3\apksigner.bat" sign --ks lmpatcher.keystore --ks-key-alias lmpatcher original.apk