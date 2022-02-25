# JavaScript クライアント SDK

_他言語バージョンもあります_：[English](README.md)

<br>

## SDK のコード例を実行する

- Node バージョン 10.13 もしくはそれ以上
- Momento オーストークンが必要です。トークン発行は[Momento CLI](https://github.com/momentohq/momento-cli)から行えます。

```bash
# npm registryを設定する
npm config set @momento:registry https://momento.jfrog.io/artifactory/api/npm/npm-public/
cd javascript
npm install

#  SDKコード例を実行する
MOMENTO_AUTH_TOKEN=<YOUR AUTH TOKEN> npm run example
```

SDK コード例: [index.ts](index.ts)

## SDK を自身のプロジェクトで使用する

### インストール方法

```bash
npm config set @momento:registry https://momento.jfrog.io/artifactory/api/npm/npm-public/
npm install @momento/sdk
```

### 使用方法

```typescript
import { SimpleCacheClient, CacheGetStatus } from "@momento/sdk";

// ユーザーのMomentoオーストークン
const authToken = process.env.MOMENTO_AUTH_TOKEN;

//  Momentoをイニシャライズする
const DEFAULT_TTL = 60; // デフォルトTTLは60秒
const momento = new SimpleCacheClient(authToken, DEFAULT_TTL);

// "myCache"という名のキャッシュを作成し、その後そのキャッシュをリターンする
const CACHE_NAME = "myCache";
const cache = await momento.createCache(CACHE_NAME);

// デフォルトTTLでキーを設定
await cache.set(CACHE_NAME, "key", "value");
const res = await cache.get(CACHE_NAME, "key");
console.log("result: ", res.text());

// TTL５秒でキーを設定
await cache.set(CACHE_NAME, "key2", "value2", 5);

// 永久にキャッシュを削除する
await momento.deleteCache(CACHE_NAME);
```

Momento はバイト型のストアもサポートしています

```typescript
const key = new Uint8Array([109, 111, 109, 101, 110, 116, 111]);
const value = new Uint8Array([
  109, 111, 109, 101, 110, 116, 111, 32, 105, 115, 32, 97, 119, 101, 115, 111,
  109, 101, 33, 33, 33,
]);
await cache.set("cache", key, value, 50);
await cache.get("cache", key);
```

キャッシュミスの対応

```typescript
const res = await cache.get("cache", "non-existent key");
if (res.status === CacheGetStatus.Miss) {
  console.log("cache miss");
}
```

ファイルのストア

```typescript
const buffer = fs.readFileSync("./file.txt");
const filebytes = Uint8Array.from(buffer);
const cacheKey = "key";
const cacheName = "my example cache";

// キャッシュにファイルをストアする
await cache.set(cacheName, cacheKey, filebytes);

// ファイルをキャッシュから取り出す
const getResp = await cache.get(cacheName, cacheKey);

// ファイルをディスクに書き込む
fs.writeFileSync("./file-from-cache.txt", Buffer.from(getResp.bytes()));
```
