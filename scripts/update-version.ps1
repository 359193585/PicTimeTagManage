# 获取最新 tag
$tag = git describe --tags --abbrev=0 2>$null
if (-not $tag) {
    Write-Host "❌ 没有找到 Git tag，使用默认版本号 v1.0.0"
    $version = "1.0.0"
} else {
    # 从 Git tag 中提取版本号，假设格式为 v1.0.0
    $version = $tag -replace '^v', ''
    Write-Host  "📦 找到 Git tag版本号: $version" 
}

# 解析版本号
$versionParts = $version.Split('.')
$major = $versionParts[0]
$minor = $versionParts[1]
$build = $versionParts[2]
$revision = [int]$versionParts[3] + 1  # 自动递增修订号

# 拼接新的版本号
$newVersion = "$major.$minor.$build.$revision"
Write-Host  "✅ 拼接新的版本号: $newVersion" 

# 更新 AssemblyInfo.cs 文件中的版本号
$assemblyInfoFiles = Get-ChildItem -Path ./src -Filter AssemblyInfo.cs -Recurse
foreach ($file in $assemblyInfoFiles) {
    Write-Host "🔧 更新版本号 -> $($file.FullName)"
  (Get-Content $file.FullName) |
    ForEach-Object {
        # 替换 AssemblyVersion 和 AssemblyFileVersion
        $_ = $_ -replace '(?<=\[assembly: AssemblyVersion\(")(\d+\.\d+\.\d+\.\d+)(?="\)\])', "$newVersion"
        $_ = $_ -replace '(?<=\[assembly: AssemblyFileVersion\(")(\d+\.\d+\.\d+\.\d+)(?="\)\])', "$newVersion"
        
        # 返回更新后的内容
        $_
    } | Set-Content $file.FullName -Encoding UTF8

}
