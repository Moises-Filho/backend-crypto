
```markdown
# CryptoMonitor API (Backend)

API REST para monitoramento de preços de criptomoedas e status de serviços, construída com **.NET 8 / C#**.

## 📂 Estrutura do Projeto
```

CryptoMonitor.Api/          # API principal
CryptoMonitor.Infrastructure/ # Acesso a dados, repositórios
CryptoMonitor.Domain/       # Entidades e regras de negócio
CryptoMonitor.Tests/        # Testes unitários com xUnit

````

## ⚙️ Tecnologias
- C# / .NET 8  
- ASP.NET Core MVC / REST API  
- Swagger UI para documentação de endpoints  
- Health Checks integrados (`/health`)  
- xUnit para testes unitários  
- Jaeger para tracing (OpenTelemetry)  
- Docker / Docker Compose  

## 🚀 Como rodar localmente

### Pré-requisitos
- .NET SDK 8
- Docker (opcional)
- SQL Server / MySQL (conforme configuração do `appsettings.json`)

### Rodando sem Docker
```bash
cd backend/CryptoMonitor.Api
dotnet restore
dotnet build
dotnet run
````

A API vai rodar em: [https://localhost:5001](https://localhost:5001)
Swagger UI: [https://localhost:5001/swagger](https://localhost:5001/swagger)
Health check: [https://localhost:5001/health](https://localhost:5001/health)

### Rodando com Docker

```bash
docker build -t crypto-monitor-api ./backend/CryptoMonitor.Api
docker run -p 5001:5001 crypto-monitor-api
```

## ✅ Testes

Executando todos os testes com xUnit:

```bash
cd backend/CryptoMonitor.Tests
dotnet test
```

## 📝 Observabilidade

* Logs estruturados no console / arquivo
* Tracing distribuído com Jaeger
* Endpoints de health check para monitoramento

## 📦 APIs Principais

* `/cryptos` → Preços das criptos
* `/health` → Status geral da API e serviços dependentes

## 🔗 Links úteis

* Swagger: `/swagger`
* Health Check: `/health`

````

---

## **Frontend – README.md**
```markdown
# CryptoMonitor Angular (Frontend)

Dashboard de criptomoedas desenvolvido com **Angular 20**, consumindo a API do backend.

## 📂 Estrutura do Projeto
````

frontend/
├─ src/                # Código fonte Angular
├─ app/
│  ├─ pages/           # Páginas (Dashboard, Health)
│  └─ components/      # Componentes reutilizáveis
├─ assets/             # Imagens e assets
├─ angular.json
└─ package.json

````

## ⚙️ Tecnologias
- Angular 20  
- TypeScript  
- Bootstrap 5 para UI e ícones  
- Vite como build tool  
- Consumo de API REST (backend)  
- Observabilidade com Health Check  
- Docker (opcional)  

## 🚀 Como rodar localmente

### Pré-requisitos
- Node.js 22+
- npm ou yarn
- Angular CLI (opcional)

### Instalando dependências
```bash
cd frontend
npm install
````

### Rodando o servidor de desenvolvimento

```bash
ng serve
```

Acesse: [http://localhost:4200](http://localhost:4200)

### Rodando com Docker

```bash
docker build -t crypto-monitor-frontend ./frontend
docker run -p 4200:4200 crypto-monitor-frontend
```

## 🔗 Conexão com Backend

Configure a variável de ambiente **API\_URL** para apontar para `https://localhost:5001`
O dashboard consome `/cryptos` e `/health` do backend

## 🎨 Funcionalidades

* Dashboard de preços de criptomoedas
* Indicadores de variação 24h
* Health check de serviços
* Atualização manual e automática
* Responsivo (desktop e mobile)

## 📦 Scripts úteis

```bash
npm run build       # Build de produção
npm run lint        # Verificação de estilo
npm run test        # Testes unitários
```
