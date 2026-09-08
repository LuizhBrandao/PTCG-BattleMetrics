# ⚡ PTCG Battle Metrics

> Progressive Web App (PWA) Mobile-First em **.NET 9 / C#** (Zero Node.js) voltada para jogadores competitivos de Pokémon TCG rastrearem histórico de partidas, torneios e análise profunda de métricas.

---

## 📱 Recursos Principais

### 1. Modo Torneio (Fast Input em < 10 segundos)
- Registro rápido de rodada entre jogos com **apenas 3 campos obrigatórios**:
  - **Deck Utilizado:** Seleção rápida ou lembrança automática do deck ativo.
  - **Arquétipo Enfrentado:** Chips do metagame competitivo em 1 toque + busca rápida (*Charizard ex, Lugia VSTAR, Gardevoir ex, Raging Bolt ex, Dragapult ex, Terapagos ex, Regidrago VSTAR, Miraidon ex, Roaring Moon ex, Snorlax Stall*).
  - **Resultado:** 3 botões táteis grandes (Vitória [Verde], Derrota [Vermelho], Empate [Amarelo]).

### 2. Progressive Disclosure (Telemetria Detalhada Opcional)
- Acordeão expansível com métricas aprofundadas:
  - Número da Rodada e Número da Mesa.
  - Nome do Adversário e POP ID.
  - Cara ou Coroa (Venceu/Perdeu) e Ordem de Turno (Começou em 1º ou 2º).
  - Contagem de Mulligans (Jogador vs Oponente).
  - Contagem de Prêmios Restantes (0 a 6).
  - Condição de Vitória / Término (Nocaute/6 Prêmios, Concede, Deck Out, Timeout/Morte Súbita, Penalidade/DQ).
  - Pokémon Ativo Inicial revelado na abertura.
  - Seleção de *Tech Cards* acionadas na partida.
  - Anotações táticas (erros de sequenciamento, cartas prizadas críticas).

### 3. Importador de Listas do Pokémon TCG Live (PTCGL)
- Parser automático para listas em formato de texto exportadas do PTCGL (em inglês e português).
- Validação automática de 60 cartas.
- Contagem e distribuição de Pokémon, Treinadores (Apoiadores, Itens, Ferramentas, Estádios) e Energias.
- Detecção automática de *Tech Cards* para rastreamento estatístico.

### 4. Motor Analítico & Matriz de Matchups
- **Visão Geral:** Win Rate geral, Win Rate sem empates, agrupado por deck e por formato.
- **Impacto de Iniciativa:** Win Rate começando em 1º vs 2º (geral e discriminado por arquétipo).
- **Matriz de Matchups:** Tabela dinâmica categorizada com badges:
  - 🟢 **Favorável** ($\ge 55\%$)
  - 🟡 **Neutro** ($45\text{-}55\%$)
  - 🔴 **Desfavorável** ($< 45\%$)
  - Média de prêmios comprados por confronto e WR de iniciativa por matchup.
- **Telemetria Avançada:**
  - Vantagem de vencer o cara-ou-coroa inicial.
  - Resistência em derrotas: média de prêmios tomados em derrotas (derrotas parelhas de 4-5 prêmios vs atropelos de 0-1 prêmio).
  - Correlação de derrotas com mulligans (0, 1 e 2+ mulligans).
  - Impacto de *tech cards* específicas no win rate (com tech vs sem tech e delta percentual).
  - Distribuição de condições de término.

### 5. Offline-First / PWA
- Totalmente instalável no smartphone (Android / iOS) via **Progressive Web App**.
- Armazenamento local e cache via `localStorage` e Service Worker para funcionamento 100% offline em lojas e convenções sem sinal.
- Sincronização automática em lote (`/api/sync`) ao reconectar.

---

## 🏗️ Arquitetura (Clean Architecture & Zero Node.js)

- **Backend:** C# / .NET 9 (ASP.NET Core Minimal APIs + Swagger UI).
- **Frontend:** Blazor WebAssembly PWA (C# nativo no navegador via WebAssembly).
- **Persistência:** SQLite via Entity Framework Core 9 com migrações automáticas e seed de dados do metagame competitivo.
- **Testes:** 9 testes unitários em xUnit cobrindo o parser de listas PTCGL e o motor de métricas.

```
PTCGBattleMetrics/
├── src/
│   ├── Domain/           # Entidades, Enums e Regras de Domínio
│   ├── Application/      # Casos de Uso, Parser PTCGL e Motor de Métricas
│   ├── Infrastructure/   # SQLite EF Core e Repositórios
│   ├── Api/              # Web API REST + Blazor WASM Host + Swagger
│   └── Client/           # Blazor WebAssembly PWA Mobile-First
└── tests/
    └── PTCGBattleMetrics.Tests/ # Testes unitários com xUnit
```

---

## 🚀 Como Executar

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (ou superior).

### Execução
A partir da raiz do projeto:

```powershell
dotnet run --project src/Api/Api.csproj --launch-profile http
```

- **PWA no Navegador:** `http://localhost:5016` (ou pelo IP da sua rede local no smartphone, ex: `http://192.168.15.92:5016`)
- **Documentação Swagger:** `http://localhost:5016/swagger`

### Executar Testes Unitários
```powershell
dotnet test
```
