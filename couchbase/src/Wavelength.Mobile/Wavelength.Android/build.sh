msbuild Wavelength.Android.csproj -t:Clean,Build,PackageForAndroid -p:Configuration=Release;AotAssemblies=true;AndroidPackageFormat=aab;OutputPath=~/Downloads/couchbid/couchbid.aab
