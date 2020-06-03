chdir /D %~dp0
del ..\..\TargetPath\AssetBundles.zip
winrar a -ep1 -k -r -s ..\..\TargetPath\AssetBundles.zip ..\DestinationPath\Android\AssetBundles