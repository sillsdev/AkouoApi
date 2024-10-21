# AkouoApi
api to pull from audio project manager data for the Akouo app
dotnet build 
dotnet lambda package --configuration release --output-package bin/release/net6.0/deploy-package_dev.zip
serverless deploy --verbose -s dev