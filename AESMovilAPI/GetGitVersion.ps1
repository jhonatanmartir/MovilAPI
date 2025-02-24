# Ruta completa al ejecutable de Git
$gitPath = "C:\Users\creativa.jmartir.c\AppData\Local\Programs\Git\bin\git.exe"

# Verifica que Git exista en la ruta especificada
if (-Not (Test-Path $gitPath)) {
    Write-Error "Git no se encontró en la ruta especificada: $gitPath"
    exit 1
}

# Obtiene la última etiqueta y su mensaje
$gitOutput = & $gitPath for-each-ref refs/tags --sort=-creatordate --count=1 --format="%(refname:short)|%(contents:subject)"
# Divide la salida en etiqueta y mensaje
$parts = $gitOutput -split '\|'
$tag = $parts[0] -replace '^v', ''  # Elimina el prefijo "v" de la versión
$message = $parts[1]
$parts = $tag -split '\s+'          # Divide por espacio
$tag = $parts[0]

# Guarda los valores en un archivo temporal
$content = @"
<Project>
  <PropertyGroup>
    <Tag>$tag</Tag>
    <Message>$message</Message>
  </PropertyGroup>
</Project>
"@

# Escribir el archivo en la raíz del proyecto
Set-Content -Path GitVersion.props -Value $content