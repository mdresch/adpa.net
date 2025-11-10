# Fix the compilation errors in Program.cs
$content = Get-Content 'Program.cs'

# Fix line 149 (index 148): DefaultCORSPolicy assignment
$content[148] = '        config.DefaultCORSPolicy = "DefaultPolicy";'

# Fix line 160 (index 159): BlockedCountries assignment  
$content[159] = '        config.BlockedCountries = string.Empty;'

# Remove the Dictionary initialization lines (lines 150-157)
$newContent = @()
for ($i = 0; $i -lt $content.Length; $i++) {
    if ($i -ge 149 -and $i -le 156) {
        # Skip the Dictionary initialization lines
        continue
    }
    $newContent += $content[$i]
}

# Also need to fix the condition on line 147
for ($i = 0; $i -lt $newContent.Length; $i++) {
    if ($newContent[$i] -like '*config.DefaultCORSPolicy == null*') {
        $newContent[$i] = '    if (string.IsNullOrEmpty(config.DefaultCORSPolicy))'
        break
    }
}

$newContent | Set-Content 'Program.cs'
Write-Host "Fixed Program.cs compilation errors"