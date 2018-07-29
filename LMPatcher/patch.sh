#!/bin/bash
zipalign -f -v 4 original.apk aligned.apk
jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore lmpatcher.keystore -storepass lmpatcher -keypass lmpatcher aligned.apk lmpatcher