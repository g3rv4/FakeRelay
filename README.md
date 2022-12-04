# What is this?

FakeRelay is a tool for Mastodon admins to load statuses into their instances.

## Why is it needed?

If you're on a small or solo instance, following a hashtag doesn't provide a lot of value. This is because you'll only see stuff that's on the instance's federated timeline... but if you're the only user, the federated timeline is the same as your timeline.

Discovering what to index isn't hard (I'm hitting big instances' `/tags/{interestingTag}.json` routes to fetch the content I want), but telling my instance that I want to bring over a toot isn't straightforward.

One thing any user can do is hit the `/api/v2/search` endpoint using `resolve=true`. That does accomplish the goal of fetching the status, but it's a synchronous call so it can take some seconds.

This project uses the ActivityPub `/inbox` endpoint as if it were a relay. And then, the request is queued and eventually processed.

## How can I use it?

### You need to get an api key

Ask the operator for an api key. If you want an api key for fakerelay.gervas.io, I'm `@g3rv4@mastodonte.tech`. Send me a toot with your instance domain and I'll get you one. The API key will be associated with your domain.

### Add the relay to your instance

Your instance will receive traffic from this site as if it were a relay. But don't worry, it won't send anything except from the statuses you tell it to index.

You need to go to `/admin/relays` and add `https://fakerelay.gervas.io/inbox` as a relay. That's it! and you can remove it whenever you want.

### Figure what you want to index

Use whatever logic you want to find stuff to index. On this example, I want to index `https://mastodonte.tech/users/g3rv4/statuses/109370844647385274`

### Do a simple request

And all you need to do is a post asking for the site to index it to your site!

```
curl -X "POST" "https://fakerelay.gervas.io/index" \
     -H 'Authorization: Bearer {apiKey}' \
     -H 'Content-Type: application/x-www-form-urlencoded; charset=utf-8' \
     --data-urlencode "statusUrl=https://mastodonte.tech/users/g3rv4/statuses/109370844647385274"
```

## I want to run this myself!

Sure thing! The easiest way is doing so via `docker-compose`.

### Setting up docker compose

I'm using this `docker-compose.yml`:

```
version: '2'
services:
  fakerelay:
    image: 'ghcr.io/g3rv4/fakerelay:1.0.3'
    command: 'web'
    hostname: fakerelay
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
      - CONFIG_PATH=/data/config.json
    restart: always
    volumes:
      - '/local/path/to/data:/data'
  cli:
    image: 'ghcr.io/g3rv4/fakerelay:1.0.2'
    volumes:
      - '/local/path/to/data:/data'
```

That will store the configuration files at `/local/path/to/data` (they are a couple json files).

### Configure the app

The first time you run this it needs to create a key, you can trigger that using:

```
docker-compose run --rm cli config {relayHost}
```

### Add authorized hosts

You can add hosts, and that will generate their tokens using the `add-host` command. That will output the key:

```
g3rv4@s1:~/docker/FakeRelay$ docker-compose run --rm cli add-host mastodon.social
Key generated for mastodon.social
vti7J0MDDw1O5EPRwfuUafJJjpErhXTwECGEvuw/G4UVWgLXtnrnmPIRRsOcvMD0juwSlvUnchIzgla030AIRw==
```

### Rotate a key

You can use `update-host` to rotate a hosts' key:

```
g3rv4@s1:~/docker/FakeRelay$ docker-compose run --rm cli update-host mastodon.social
Key generated for mastodon.social
wpSX9xpPgX0gjgAxO0Jc+GLSOXubVgv73FOvAihR2EmgK/AfDHz21sF72uqrLnVGzcq2BDXosMeKdFR76q6fpg==
```

### Remove a host

If you want to revoke a host's key, you can use `delete-host`:

```
g3rv4@s1:~/docker/FakeRelay$ docker-compose run --rm cli delete-host mastodon.social
Key deleted for mastodon.social
```