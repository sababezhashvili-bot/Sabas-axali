$file = 'c:\Users\Sabab\Desktop\Racha629\racha-frontend\src\views\MapView.vue'
$lines = [System.IO.File]::ReadAllLines($file, [System.Text.Encoding]::UTF8)

# Lines 551-582 (0-indexed: 550-581) contain the old manual hole-ring builder
# We'll replace them with the Turf buffer+difference block
$startIdx = 550  # 0-indexed = line 551
$endIdx = 581  # 0-indexed = line 582 (inclusive)

$newLines = @(
    '        // -- Geometry B: 500m buffer for crust (wider than satellite extent) --',
    '        // The mask hole is punched at the BUFFERED shape, so the white void stops',
    '        // 500m outside Racha, exposing a visible dark crust ledge.',
    '        const turf = window.turf',
    '        let crustFeat   = rachaFeat   // fallback if Turf not loaded',
    '        let maskCutFeat = rachaFeat',
    '        if (turf) {',
    "          try {",
    "            crustFeat   = turf.buffer(rachaFeat, 0.5, { units: 'kilometers' })",
    '            maskCutFeat = crustFeat',
    "          } catch(e) { console.warn('turf.buffer failed, using original', e) }",
    '        }',
    '',
    '        // -- World minus BUFFERED Racha: mask stops at crust outer edge --',
    '        let cutoutMaskData = null',
    '        if (turf && turf.difference) {',
    '          try {',
    '            const worldPoly = turf.polygon([[',
    '              [-180,-85.051],[180,-85.051],[180,85.051],[-180,85.051],[-180,-85.051]',
    '            ]])',
    '            const diff = turf.difference(worldPoly, maskCutFeat)',
    "            cutoutMaskData = { type:'FeatureCollection', features:[diff] }",
    "          } catch(e) { console.warn('turf.difference failed, using manual fallback', e) }",
    '        }',
    '        if (!cutoutMaskData) {',
    '          const geom2 = maskCutFeat.geometry',
    "          const holeRings = geom2.type === 'MultiPolygon'",
    '            ? geom2.coordinates.map(p => p[0])',
    '            : [geom2.coordinates[0]]',
    '          cutoutMaskData = {',
    "            type:'FeatureCollection', features:[{",
    "              type:'Feature',",
    "              geometry:{ type:'Polygon', coordinates:[",
    '                [[-180,-85.051],[180,-85.051],[180,85.051],[-180,85.051],[-180,-85.051]],',
    '                ...holeRings.map(ring => {',
    '                  const flat = [...ring]; let area = 0',
    '                  for (let i=0, j=flat.length-1; i<flat.length; j=i++)',
    '                    area += (flat[j][0]+flat[i][0])*(flat[j][1]-flat[i][1])',
    '                  return area > 0 ? flat.reverse() : flat',
    '                })',
    "              ]}",
    '            }]',
    '          }',
    '        }'
)

# Splice: keep everything before startIdx, insert new lines, then everything after endIdx
$before = $lines[0..($startIdx - 1)]
$after = $lines[($endIdx + 1)..($lines.Count - 1)]
$result = $before + $newLines + $after

[System.IO.File]::WriteAllLines($file, $result, [System.Text.Encoding]::UTF8)
Write-Host "SUCCESS: Replaced lines $($startIdx+1) to $($endIdx+1), total now $($result.Count)"
