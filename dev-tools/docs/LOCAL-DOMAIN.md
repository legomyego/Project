# 🌐 Локальные домены для разработки

Вместо неудобных портов можно использовать красивые домены:

## Было:
```
http://localhost:3000       → Main App
http://localhost:5173       → Admin Panel
http://localhost:5010       → API
http://192.168.1.14:3000    → С телефона
```

## Стало:
```
http://recipes.local              → Main App
http://admin.recipes.local        → Admin Panel
http://api.recipes.local          → API
http://api.recipes.local/swagger  → Swagger
```

---

## 🚀 Быстрая установка (один скрипт)

```bash
./setup-local-domain.sh
```

Скрипт автоматически:
1. ✅ Установит nginx (если не установлен)
2. ✅ Настроит reverse proxy
3. ✅ Обновит /etc/hosts
4. ✅ Запустит nginx

---

## 🛠 Ручная установка (если хотите понять, как это работает)

### Шаг 1: Установите nginx

```bash
brew install nginx
```

### Шаг 2: Скопируйте конфигурацию

```bash
sudo cp nginx-local.conf /usr/local/etc/nginx/servers/recipes.conf
```

### Шаг 3: Обновите /etc/hosts

```bash
sudo nano /etc/hosts
```

Добавьте:
```
127.0.0.1       recipes.local
127.0.0.1       api.recipes.local
127.0.0.1       admin.recipes.local
```

### Шаг 4: Перезапустите nginx

```bash
brew services restart nginx
```

---

## 📱 Доступ с других устройств

### С Mac/PC в той же сети:

Добавьте в их /etc/hosts (Windows: C:\Windows\System32\drivers\etc\hosts):
```
192.168.1.14    recipes.local
192.168.1.14    api.recipes.local
192.168.1.14    admin.recipes.local
```

### С телефона/планшета:

К сожалению, на iOS/Android без root нельзя редактировать hosts файл.

**Варианты:**
1. **Используйте IP с портом:** http://192.168.1.14:3000
2. **Настройте локальный DNS сервер** (продвинуто)
3. **Используйте сервис типа ngrok** (для демо)

---

## 🔧 Как это работает?

```
┌─────────────────────────────────────────────────┐
│  Браузер: http://recipes.local                 │
└──────────────────┬──────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────┐
│  /etc/hosts: recipes.local → 127.0.0.1         │
└──────────────────┬──────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────┐
│  nginx (порт 80):                               │
│  ├─ recipes.local → proxy → localhost:3000     │
│  ├─ admin.recipes.local → proxy → :5173        │
│  └─ api.recipes.local → proxy → :5010          │
└──────────────────┬──────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────┐
│  Ваши dev серверы:                              │
│  ├─ Nuxt (3000)                                 │
│  ├─ React Admin (5173)                          │
│  └─ .NET API (5010)                             │
└─────────────────────────────────────────────────┘
```

---

## 🎯 Преимущества

✅ **Удобнее:** `recipes.local` вместо `localhost:3000`
✅ **Профессиональнее:** как в проде
✅ **Без портов:** все на 80 порту
✅ **Поддомены:** логичная структура
✅ **HMR работает:** nginx правильно проксирует WebSocket

---

## 🐛 Troubleshooting

### "Cannot connect to recipes.local"

Проверьте nginx:
```bash
brew services list | grep nginx
# Должно быть: nginx started
```

Проверьте /etc/hosts:
```bash
cat /etc/hosts | grep recipes
```

### "502 Bad Gateway"

Dev серверы не запущены. Запустите:
```bash
npm run dev
```

### Очистить кеш DNS

```bash
sudo dscacheutil -flushcache
sudo killall -HUP mDNSResponder
```

### Посмотреть логи nginx

```bash
tail -f /usr/local/var/log/nginx/error.log
tail -f /usr/local/var/log/nginx/access.log
```

---

## 🗑 Удаление

Если хотите вернуть всё как было:

```bash
# Остановить nginx
brew services stop nginx

# Удалить конфигурацию
sudo rm /usr/local/etc/nginx/servers/recipes.conf

# Удалить записи из /etc/hosts
sudo nano /etc/hosts
# Удалите строки с recipes.local

# (Опционально) Удалить nginx
brew uninstall nginx
```

---

## 💡 Альтернативы

### 1. Traefik (автоматический reverse proxy)
```yaml
# Настраивается через docker labels
# Подходит если используете Docker Compose
```

### 2. Caddy (автоматический HTTPS)
```
# Проще nginx, автоматически получает SSL сертификаты
```

### 3. ngrok (публичный URL)
```bash
# Для демо внешним людям
ngrok http 3000
# Даст: https://random-id.ngrok.io
```
