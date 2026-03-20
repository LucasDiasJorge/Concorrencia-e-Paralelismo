# ⚛️ Reactor Pattern

Padrão para demultiplexação de eventos de I/O. Um único thread monitora múltiplas fontes de eventos e despacha para handlers apropriados.

---

## 📋 Problema

Servidor precisa lidar com milhares de conexões simultâneas. Modelo "thread por conexão" não escala:

```
Thread-per-connection:
Connection 1 ──→ Thread 1 (90% esperando I/O)
Connection 2 ──→ Thread 2 (90% esperando I/O)
...
Connection 10000 ──→ Thread 10000 💥 (out of memory)
```

## ✅ Solução

Um thread monitora TODAS as conexões e só acorda quando há trabalho:

```
Reactor Pattern:
                     ┌──→ Handler A
Connection 1 ──┐     │
Connection 2 ──┼──→ [Event Loop] ──→ Handler B  
Connection N ──┘     │
                     └──→ Handler C

1 thread gerencia milhares de conexões!
```

---

## 💻 Implementações

### Conceito Base (Pseudo-código)
```
while (running) {
    events = demultiplexer.select()  // Bloqueia até eventos
    for event in events:
        handler = handlers[event.source]
        handler.handle(event)
}
```

### C# - Com Socket e Select
```csharp
public class SimpleReactor
{
    private readonly Dictionary<Socket, IEventHandler> _handlers = new();
    private readonly List<Socket> _sockets = new();
    private bool _running = true;

    public void Register(Socket socket, IEventHandler handler)
    {
        _sockets.Add(socket);
        _handlers[socket] = handler;
    }

    public void Run()
    {
        while (_running)
        {
            var readList = new List<Socket>(_sockets);
            
            Socket.Select(readList, null, null, 1000);  // Demultiplexer
            
            foreach (var socket in readList)
            {
                _handlers[socket].HandleEvent(socket);  // Dispatch
            }
        }
    }
}

public interface IEventHandler
{
    void HandleEvent(Socket socket);
}
```

### Node.js - Event Loop nativo
```javascript
// Node.js é construído sobre o Reactor Pattern!
const net = require('net');

const server = net.createServer((socket) => {
    // Handler para nova conexão
    socket.on('data', (data) => {
        // Handler para dados recebidos
        socket.write(`Echo: ${data}`);
    });
    
    socket.on('end', () => {
        // Handler para desconexão
        console.log('Client disconnected');
    });
});

server.listen(8080);  // Event loop gerencia tudo
```

### Java - NIO Selector
```java
import java.nio.*;
import java.nio.channels.*;

public class Reactor implements Runnable {
    private final Selector selector;
    private final ServerSocketChannel serverChannel;

    public Reactor(int port) throws IOException {
        selector = Selector.open();
        serverChannel = ServerSocketChannel.open();
        serverChannel.socket().bind(new InetSocketAddress(port));
        serverChannel.configureBlocking(false);
        serverChannel.register(selector, SelectionKey.OP_ACCEPT, new Acceptor());
    }

    public void run() {
        while (!Thread.interrupted()) {
            selector.select();  // Bloqueia até eventos
            
            Set<SelectionKey> selected = selector.selectedKeys();
            for (SelectionKey key : selected) {
                dispatch(key);
            }
            selected.clear();
        }
    }

    private void dispatch(SelectionKey key) {
        Runnable handler = (Runnable) key.attachment();
        if (handler != null) {
            handler.run();
        }
    }

    class Acceptor implements Runnable {
        public void run() {
            try {
                SocketChannel channel = serverChannel.accept();
                if (channel != null) {
                    new Handler(selector, channel);
                }
            } catch (IOException ex) { }
        }
    }
}
```

### Python - asyncio (Reactor moderno)
```python
import asyncio

async def handle_client(reader, writer):
    while True:
        data = await reader.read(1024)
        if not data:
            break
        writer.write(f"Echo: {data.decode()}".encode())
        await writer.drain()
    writer.close()

async def main():
    server = await asyncio.start_server(handle_client, '127.0.0.1', 8080)
    async with server:
        await server.serve_forever()

asyncio.run(main())  # Event loop gerencia tudo
```

### C - libuv (usado pelo Node.js)
```c
#include <uv.h>

void on_read(uv_stream_t* stream, ssize_t nread, const uv_buf_t* buf) {
    if (nread > 0) {
        // Handle data
        uv_write_t* req = malloc(sizeof(uv_write_t));
        uv_buf_t wrbuf = uv_buf_init(buf->base, nread);
        uv_write(req, stream, &wrbuf, 1, NULL);
    }
}

void on_connection(uv_stream_t* server, int status) {
    uv_tcp_t* client = malloc(sizeof(uv_tcp_t));
    uv_tcp_init(uv_default_loop(), client);
    uv_accept(server, (uv_stream_t*)client);
    uv_read_start((uv_stream_t*)client, alloc_buffer, on_read);
}

int main() {
    uv_loop_t* loop = uv_default_loop();
    uv_tcp_t server;
    
    uv_tcp_init(loop, &server);
    uv_tcp_bind(&server, ...);
    uv_listen((uv_stream_t*)&server, 128, on_connection);
    
    return uv_run(loop, UV_RUN_DEFAULT);  // Event loop
}
```

---

## 🎯 Componentes do Padrão

```
┌───────────────────────────────────────────────────────┐
│                      REACTOR                           │
├───────────────────────────────────────────────────────┤
│                                                        │
│  ┌─────────────────┐     ┌─────────────────────────┐  │
│  │  Synchronous    │     │    Event Handlers       │  │
│  │  Event          │     │                         │  │
│  │  Demultiplexer  │     │  AcceptHandler          │  │
│  │                 │     │  ReadHandler            │  │
│  │  (select/poll/  │────▶│  WriteHandler           │  │
│  │   epoll/kqueue) │     │  TimeoutHandler         │  │
│  │                 │     │                         │  │
│  └─────────────────┘     └─────────────────────────┘  │
│           │                         │                  │
│           ▼                         ▼                  │
│  ┌─────────────────┐     ┌─────────────────────────┐  │
│  │  Handle/Socket  │     │  Dispatcher             │  │
│  │  Registry       │◀───▶│  (maps events to        │  │
│  │                 │     │   handlers)             │  │
│  └─────────────────┘     └─────────────────────────┘  │
│                                                        │
└───────────────────────────────────────────────────────┘
```

---

## 📊 Reactor vs Thread-per-Connection

| Aspecto | Thread-per-Connection | Reactor |
|---------|----------------------|---------|
| Threads | 1 por conexão | 1 (ou poucos) |
| Memória | Alta (stack por thread) | Baixa |
| Conexões | ~1K-10K | ~100K+ (C10K) |
| Context Switch | Alto | Baixo |
| Complexidade | Simples | Maior |
| CPU-bound work | Bom | Ruim (bloqueia loop) |

---

## ⚠️ Cuidados

### 1. Nunca bloquear o Event Loop
```javascript
// ❌ Bloqueia TODO o servidor!
app.get('/slow', (req, res) => {
    const data = fs.readFileSync('huge-file.txt');  // SYNC!
    res.send(data);
});

// ✅ Use async
app.get('/slow', async (req, res) => {
    const data = await fs.promises.readFile('huge-file.txt');
    res.send(data);
});
```

### 2. CPU-bound work precisa de Worker Pool
```javascript
// ❌ Cálculo pesado bloqueia o loop
app.get('/hash', (req, res) => {
    const hash = computeExpensiveHash(req.body);  // BLOQUEIA!
    res.send(hash);
});

// ✅ Delegue para worker thread
const { Worker } = require('worker_threads');

app.get('/hash', (req, res) => {
    const worker = new Worker('./hash-worker.js', { workerData: req.body });
    worker.on('message', hash => res.send(hash));
});
```

---

## 🔄 Variações

| Variação | Descrição | Exemplo |
|----------|-----------|---------|
| **Single-threaded** | 1 thread faz tudo | Node.js |
| **Multi-threaded** | Pool de threads + reactor | Netty |
| **Multi-reactor** | 1 reactor por core | Nginx |

---

## 🔗 Padrões Relacionados

- [Proactor](../Proactor/) - Versão assíncrona do Reactor
- [Thread Pool](../ThreadPool/) - Usado junto para CPU-bound
- [Active Object](../ActiveObject/) - Handlers como active objects

---

## 📚 Referências

- [Reactor pattern - Wikipedia](https://en.wikipedia.org/wiki/Reactor_pattern)
- [Douglas Schmidt - Reactor Pattern Paper](http://www.dre.vanderbilt.edu/~schmidt/PDF/reactor-siemens.pdf)
- [The C10K problem](http://www.kegel.com/c10k.html)
- [libuv Design Overview](http://docs.libuv.org/en/v1.x/design.html)
