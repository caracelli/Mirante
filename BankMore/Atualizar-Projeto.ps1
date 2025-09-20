# Caminho raiz da solução
$root = "C:\Projetos\BankMore"

# Lista de projetos
$projects = @(
    "BankMore.Domain",
    "BankMore.Data",
    "BankMore.Application",
    "BankMore.Api",
    "BankMore.Tests",
    "BankMore.Services"
)

# Pacotes a alinhar
$packages = @{
    "MediatR" = "11.1.0"
    "MediatR.Extensions.Microsoft.DependencyInjection" = "11.1.0"
    "KafkaFlow" = "3.8.0"
    "Microsoft.Data.Sqlite" = "8.0.0"
    "Dapper" = "2.1.0"
    "Moq" = "4.20.70"
}

# Função para atualizar ou instalar pacote NuGet
function Add-OrUpdatePackage([string]$projPath, [string]$pkg, [string]$version) {
    Write-Host "Alinhando pacote $pkg@$version em $projPath"
    dotnet add $projPath package $pkg --version $version --no-restore
}

# Limpar bin/obj
foreach ($proj in $projects) {
    $projPath = Join-Path $root $proj
    Write-Host "`n=== Limpando $projPath ==="
    Remove-Item -Recurse -Force (Join-Path $projPath "bin") -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force (Join-Path $projPath "obj") -ErrorAction SilentlyContinue
}

# Atualizar pacotes
foreach ($proj in $projects) {
    $projPath = Join-Path $root $proj
    foreach ($pkg in $packages.Keys) {
        Add-OrUpdatePackage $projPath $pkg $packages[$pkg]
    }
}

# Restaurar pacotes
Write-Host "`n=== Restaurando pacotes NuGet ==="
dotnet restore $root

# Compilar projetos na ordem correta
$buildOrder = @(
    "BankMore.Domain",
    "BankMore.Data",
    "BankMore.Application",
    "BankMore.Services",
    "BankMore.Api",
    "BankMore.Tests"
)

foreach ($proj in $buildOrder) {
    $projPath = Join-Path $root $proj
    Write-Host "`n=== Compilando $projPath ==="
    dotnet build $projPath -c Debug
}

Write-Host "`n=== Todos os projetos foram alinhados e compilados ==="
