# LittleSoftChat Docker 部署指南

本文件說明如何使用 Docker 部署 LittleSoftChat 聊天應用程式。

## 目錄結構

```
docker/
├── backend/
│   └── docker-compose.yml    # 後端 API 服務配置
├── mysql/
│   └── docker-compose.yml    # 資料庫與快取服務配置
└── README.md                 # 本文件
```

## 系統架構

LittleSoftChat 是一個基於微服務架構的即時聊天應用程式，包含以下服務：

### 資料庫層
- **MySQL 8.0**: 主要資料庫
- **Database Migration**: 資料庫遷移工具

### 後端服務層
- **User Service**: 使用者管理服務 (HTTP: 5001, gRPC: 5011)
- **Chat Service**: 聊天服務 (HTTP: 5002, gRPC: 5012)
- **Notification Service**: 通知服務 (HTTP: 5003, SignalR Hub)
- **Gateway API**: API 閘道 (HTTP: 5000)

## 網絡配置

### 手動預先創建網絡

如果您希望手動管理網絡，請在啟動服務前執行：

```bash
# 創建共享網絡
docker network create \
  --driver bridge \
  --subnet=172.20.0.0/16 \
  --gateway=172.20.0.1 \
  little-soft-chat-network
```

## 快速部署

### 1. 環境要求

- Docker Engine 20.0+
- Docker Compose 2.0+
- 至少 4GB 可用內存
- 港口 3306, 5000-5003 未被占用

### 2. 一鍵部署（推薦）

在項目根目錄執行：

```bash
# 切換到 docker 目錄
cd docker

# 啟動資料庫服務
docker-compose -f mysql/docker-compose.yml up -d

# 等待資料庫啟動完成（約 30 秒）
echo "等待 MySQL 啟動..."
sleep 30

# 啟動後端服務
docker-compose -f backend/docker-compose.yml up -d
```

## 服務端點

部署完成後，各服務可通過以下端點訪問：

### 對外服務
- **主要 API**: http://localhost:5000 (Gateway)
- **健康檢查**: http://localhost:5000/health

### 內部服務（開發/測試用）
- **User Service**: http://localhost:5001
- **Chat Service**: http://localhost:5002  
- **Notification Service**: http://localhost:5003

### 資料庫連接
- **MySQL**: localhost:3306
  - 資料庫: `LittleSoftChatDB`
  - 使用者: `chatuser` / 密碼: `chatpass123`
  - Root: `root` / 密碼: `root123456`

## 服務配置

### JWT 設定
所有服務使用統一的 JWT 配置：
- **Secret**: `YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789`
- **Issuer**: `LittleSoftChat`
- **Audience**: `LittleSoftChat.Client`
- **過期時間**: 24 小時

### SignalR 配置
Notification Service 提供 SignalR Hub：
- **Hub 路徑**: `/chatHub`
- **Keep Alive**: 15 秒
- **Client Timeout**: 30 秒

## 監控與維護

### 檢查服務狀態

```bash
# 查看所有容器狀態
docker ps

# 檢查特定服務日誌
docker logs little-soft-chat-gateway
docker logs little-soft-chat-user-service
docker logs little-soft-chat-chat-service
docker logs little-soft-chat-notification-service

# 檢查資料庫日誌
docker logs little-soft-chat-mysql
```

### 服務健康檢查

所有後端服務都配置了健康檢查：

```bash
# 檢查服務健康狀態
docker-compose -f backend/docker-compose.yml ps
docker-compose -f mysql/docker-compose.yml ps

# 手動健康檢查
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
```

### 查看網絡狀態

```bash
# 列出所有網絡
docker network ls

# 檢查網絡詳情
docker network inspect little-soft-chat-network

# 查看網絡中的容器
docker network inspect little-soft-chat-network \
  --format='{{range .Containers}}{{.Name}}: {{.IPv4Address}}{{"\n"}}{{end}}'
```

## 故障排除

### 常見問題

#### 1. 端口被占用
```bash
# 檢查端口占用
lsof -i :5000
lsof -i :3306

# 停止衝突的服務或修改端口映射
```

#### 2. 資料庫連接失敗
```bash
# 檢查 MySQL 容器狀態
docker logs little-soft-chat-mysql

# 手動測試資料庫連接
docker exec -it little-soft-chat-mysql mysql -u chatuser -pchatpass123 LittleSoftChatDB
```

#### 3. 服務間通信失敗
```bash
# 測試容器間網絡連通性
docker exec little-soft-chat-gateway ping user-service
docker exec little-soft-chat-gateway ping mysql

# 檢查網絡配置
docker network inspect little-soft-chat-network
```

#### 4. 內存不足
```bash
# 檢查 Docker 資源使用
docker stats

# 清理未使用的資源
docker system prune -a
```

### 重置環境

如果需要完全重置環境：

```bash
# 停止所有服務
docker-compose -f backend/docker-compose.yml down
docker-compose -f mysql/docker-compose.yml down

# 刪除所有相關容器
docker rm -f $(docker ps -aq --filter "name=little-soft-chat")

# 刪除數據卷（會丟失資料庫數據）
docker volume rm docker_mysql_data

# 刪除網絡（如果是手動創建的）
docker network rm little-soft-chat-network

# 重新部署
docker-compose -f mysql/docker-compose.yml up -d
sleep 30
docker-compose -f backend/docker-compose.yml up -d
```

## 開發環境

### 本地開發配置

如果要在本地開發環境中運行部分服務：

```bash
# 只啟動資料庫
docker-compose -f mysql/docker-compose.yml up -d

# 在本地 IDE 中運行後端服務，連接到 Docker 中的資料庫
# MySQL: localhost:3306
```

### 調試模式

啟用詳細日誌輸出：

```bash
# 以前台模式運行，查看即時日誌
docker-compose -f backend/docker-compose.yml up

# 或者查看特定服務的日誌
docker-compose -f backend/docker-compose.yml logs -f gateway-api
```

## 生產環境注意事項

⚠️ **安全警告**: 當前配置僅適用於開發和測試環境

### 生產環境建議

1. **密碼安全**:
   - 更換所有預設密碼
   - 使用環境變數或 Docker Secrets
   - 使用更強的 JWT Secret

2. **網絡安全**:
   - 使用反向代理 (Nginx/Traefik)
   - 啟用 HTTPS
   - 限制內部服務端口暴露

3. **資料持久化**:
   - 配置外部資料庫
   - 設定備份策略
   - 使用持久化存儲

4. **監控與日誌**:
   - 集成 ELK Stack 或 Prometheus
   - 配置日誌轉發
   - 設定告警機制

## 相關文件

- [Backend Docker Compose](./backend/docker-compose.yml)
- [MySQL Docker Compose](./mysql/docker-compose.yml)
- [API 文檔](../src/Gateway/LittleSoftChat.Gateway.API/api-documentation.md)
- [需求文件](../需求文件.md)
