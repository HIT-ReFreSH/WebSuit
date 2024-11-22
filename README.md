# HitRefresh.WebSuit

## Connection Workflow

For Workers:

1. Request -> Peer Created
2. Auth Message -> Peer Activated/Peer Dead
3. WebSuit Output/Response/Read Message -> Forward
4. Disconnection Message -> Forward & Close

For Consumers:

1. Request -> Peer Created
2. Auth Message -> Peer Activated/Peer Dead -> Pair Worker
3. WebSuit Input/Request/Abort Message -> Forward (Open Session)
4. Disconnection Message -> Forward & Close