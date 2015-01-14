cls

$PSConfig = "C:\Users\Soumen_2\Desktop\POC\DependencyHealthCheck\DependencyHealthCheck\DependencyDetails.config"

$envParam = "DEV"
$appGroupParam = "eCommerce"
$appParam = "RIM"
$result = ""

if ($args.Count -gt 0) { $envParam = $args[0] }
if ($args.Count -gt 1) { $appGroupParam = $args[1] }
if ($args.Count -gt 2) { $appParam = $args[2] }

$PSConfigContent = New-Object Xml
$PSConfigContent.Load($PSConfig)

foreach ($env in $PSConfigContent.configuration.environments.environment) {
    if ($env.name -ne $envParam) {continue}
        
    foreach ($appGroup in $env.appGroups.appGroup) {
        if ($appGroup.name -ne $appGroupParam) {continue}

        foreach ($app in $appGroup.app) {
            if ($app.name -ne $appParam) {continue}

            foreach ($dep in $app.dependencies.dependency) {
                $result = ""
                $result = "{'Env' : '" + $env.name + "', 'AppGroup' : '" + $appGroup.name + 
                            "', 'AppName' : '" + $app.name + "', 'Dep' : '" + $dep.type + 
                            "', 'Name' : '" + $dep.name + "', 'Uri' : '" + $dep.uri

                if (($dep.type -eq "REST") -or ($dep.type -eq "CDN")) {
                    #[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
                    $webClient = New-Object System.Net.WebClient
                    
                    try {
                        $StatusJSON = $webClient.DownloadString($dep.uri)
                        $result = $result + "', 'Health' : 'GREEN' }"
                    }
                    catch {
                        if ($_.Exception.Message.Contains("404")) {
                            $result = $result + "', 'Health' : 'RED' }"
                        }
                        else {
                            $result = $result + "', 'Health' : 'GREEN' }"
                        }
                    }
                    finally {
                        $result
                    }
                }
            }
        }
    }
}