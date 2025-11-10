# Fix the specific compilation errors in Program.cs
$content = Get-Content 'Program.cs'

# Find and fix the DefaultCORSPolicy block (lines around 147-156)
for ($i = 0; $i -lt $content.Length; $i++) {
    # Change the condition from == null to string check
    if ($content[$i] -match 'if \(config\.DefaultCORSPolicy == null\)') {
        $content[$i] = '    if (string.IsNullOrEmpty(config.DefaultCORSPolicy))'
        Write-Host "Fixed line $($i+1): DefaultCORSPolicy condition"
    }
    
    # Replace the Dictionary assignment with string assignment
    if ($content[$i] -match 'config\.DefaultCORSPolicy = new Dictionary') {
        $content[$i] = '        config.DefaultCORSPolicy = "DefaultPolicy";'
        Write-Host "Fixed line $($i+1): DefaultCORSPolicy assignment"
        
        # Remove the dictionary initialization lines that follow
        $j = $i + 1
        while ($j -lt $content.Length -and $content[$j] -notmatch '^\s*\};\s*$') {
            $content[$j] = ""  # Clear the line
            $j++
        }
        if ($j -lt $content.Length) {
            $content[$j] = ""  # Clear the closing }; line too
        }
    }
    
    # Fix BlockedCountries assignment
    if ($content[$i] -match 'config\.BlockedCountries = Array\.Empty') {
        $content[$i] = '        config.BlockedCountries = string.Empty;'
        Write-Host "Fixed line $($i+1): BlockedCountries assignment"
    }
}

# Remove empty lines and write back
$filteredContent = $content | Where-Object { $_.Trim() -ne "" }
$filteredContent | Set-Content 'Program.cs'

Write-Host "Successfully fixed Program.cs compilation errors"