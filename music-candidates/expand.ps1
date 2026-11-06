$ErrorActionPreference = 'Stop'
$blockedIds = @()
$reportPath = Join-Path $PSScriptRoot 'embed-report.json'
if (Test-Path $reportPath) {
    $blockedIds = @((Get-Content -Raw $reportPath | ConvertFrom-Json).removed)
}
$videoPattern = '<a href="(?:\.\./)?video/([\w-]{11})\.html">(.*?)</a>'
$youtubePattern = '<a href="https?://(?:www\.)?youtube.com/watch\?v=([\w-]{11})"[^>]*>(.*?)</a>'

function Get-Entries([string]$url, [string]$artist = '', [string]$pattern = $videoPattern) {
    $html = (Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30).Content
    foreach ($match in [regex]::Matches($html, $pattern, 'Singleline')) {
        $title = [System.Net.WebUtility]::HtmlDecode(($match.Groups[2].Value -replace '<[^>]+>', '')).Trim()
        if ($title.Length -lt 6) { continue }
        [pscustomobject]@{ videoId=$match.Groups[1].Value; title=$title; artist=$artist; sourceUrl=$url }
    }
}

function Write-Collection([string]$code, [array]$sources, [string]$excludePattern) {
    $path = Join-Path $PSScriptRoot "$code.json"
    $old = Get-Content -Raw $path | ConvertFrom-Json
    $selected = [System.Collections.Generic.List[object]]::new()
    $ids = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($item in ($old.tracks | Select-Object -First 30)) {
        if ($item.title -match '予告編|オープニングムービー|演唱會精選' -or $item.videoId -in $blockedIds) { continue }
        if ($ids.Add($item.videoId)) { $selected.Add($item) }
    }
    $pools = @()
    foreach ($source in $sources) {
        $pattern = if ($source.pattern) { $source.pattern } else { $videoPattern }
        $entries = @(Get-Entries $source.url $source.artist $pattern)
        $pools += [pscustomobject]@{ entries=$entries; cap=$source.cap }
        Write-Output "$code source $($source.url): $($entries.Count) entries"
    }
    foreach ($pool in $pools) {
        $added = 0
        foreach ($item in $pool.entries) {
            if ($selected.Count -ge 130 -or $added -ge $pool.cap) { break }
            if ($item.title -match $excludePattern) { continue }
            if ($item.videoId -in $blockedIds) { continue }
            if ($code -eq 'zh' -and $item.title -notmatch '(?i)MV|Music Video|Official Audio|官方|歌詞版|Lyrics|\[HD\]') { continue }
            if (-not $ids.Add($item.videoId)) { continue }
            $selected.Add([pscustomobject]@{
                videoId=$item.videoId
                title=$item.title
                artist=$item.artist
                youtubeUrl="https://www.youtube.com/watch?v=$($item.videoId)"
                sourceUrl=$item.sourceUrl
            })
            $added++
        }
    }
    if ($selected.Count -ne 130) { throw "${code}: found $($selected.Count) songs, need 130" }
    $data = [ordered]@{
        language=$code
        collectedOn='2026-11-06'
        selectionNote='130 bài được tuyển từ video nổi bật và các kênh nghệ sĩ; thứ tự không phải xếp hạng chất lượng.'
        tracks=@($selected)
    }
    [System.IO.File]::WriteAllText($path, (ConvertTo-Json -InputObject $data -Depth 6), [System.Text.UTF8Encoding]::new($false))
}

$vnSources = @(
    @{url='https://kworb.net/youtube/topvideos_vietnamese.html'; artist=''; cap=25},
    @{url='https://kworb.net/youtube/artist/sontungm-tp.html'; artist='Sơn Tùng M-TP'; cap=20},
    @{url='https://kworb.net/youtube/artist/ho%C3%A0ngthu%E1%BB%B3linh.html'; artist='Hoàng Thùy Linh'; cap=25},
    @{url='https://kworb.net/youtube/artist/m%E1%BB%B9t%C3%A2m.html'; artist='Mỹ Tâm'; cap=25},
    @{url='https://kworb.net/youtube/artist/b%C3%ADchph%C6%B0%C6%A1ng.html'; artist='Bích Phương'; cap=25},
    @{url='https://kworb.net/youtube/artist/h%C6%B0%C6%A1ngtr%C3%A0m.html'; artist='Hương Tràm'; cap=25},
    @{url='https://kworb.net/youtube/artist/justatee.html'; artist='JustaTee'; cap=20}
)
Write-Collection 'vi' $vnSources '(?i)trailer|teaser|behind|making|karaoke|remix|live|episode|tập [0-9]|shorts?|reaction|phỏng vấn|hậu trường|instrumental|playlist|tuyển tập|parody|commercial|quảng cáo|dance practice|tutorial|stream'

$enSources = @(@{url='https://kworb.net/youtube/topvideos_anglophone.html'; artist=''; cap=110})
Write-Collection 'en' $enSources '(?i)trailer|teaser|behind|making|karaoke|remix|live|episode|reaction|instrumental|playlist|parody|commercial|dance practice|interview|compilation|full album'

$jaSources = @(
    @{url='https://kworb.net/youtube/topvideos_japanese.html'; artist=''; cap=45},
    @{url='https://kworb.net/youtube/artist/yoasobi.html'; artist='YOASOBI'; cap=35},
    @{url='https://kworb.net/youtube/artist/kenshiyonezu.html'; artist='Kenshi Yonezu'; cap=35},
    @{url='https://kworb.net/youtube/artist/ado.html'; artist='Ado'; cap=35},
    @{url='https://kworb.net/youtube/artist/mrsgreenapple.html'; artist='Mrs. GREEN APPLE'; cap=35}
)
Write-Collection 'ja' $jaSources '(?i)trailer|teaser|behind|making|karaoke|remix|live|episode|reaction|instrumental|playlist|commercial|dance practice|interview|shorts?|cover|digest|予告編|オープニングムービー|メイキング|カラオケ|ライブ映像|ティザー|歌ってみた'

$zhSources = @(
    @{url='https://kworb.net/youtube/artist/jaychou.html'; artist='Jay Chou'; cap=70},
    @{url='https://kworb.net/youtube/artist/gem.html'; artist='G.E.M.'; cap=40},
    @{url='https://kworb.net/youtube/trending/tw.html'; artist=''; pattern=$youtubePattern; cap=20}
)
Write-Collection 'zh' $zhSources '(?i)trailer|teaser|behind|making|karaoke|remix|live|episode|reaction|instrumental|playlist|commercial|dance practice|interview|shorts?|concert|tour|宣傳|預告|花絮|現場版|精選|fan made|challenge|冒充'
