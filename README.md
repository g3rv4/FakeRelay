# What is this?

FakeRelay is a tool for Mastodon admins to load statuses into their instances.

More importantly, FakeRelay is NOT:

* A relay. Even if from Mastodon's point of view it is a relay, if multiple instances are connected to it *it won't* share their posts.
* A scraper. FakeRelay receives a post's url and sends that to the registered instance.

## Why is it needed?

If you're an admin of an instance, it's not really that easy to tell it "hey, index this post!". One thing any user can do is hit the `/api/v2/search` endpoint using `resolve=true`. That does accomplish the goal of fetching the post, but it's a synchronous call so it can take some seconds.

FakeRelay exposes an API to use the ActivityPub `/inbox` endpoint as if it were a relay. And then, the request is queued and eventually processed.

## What can I use it for?

I use it to load content with #hashtags I care about. How? well, that's a separate project: [GetMoarFediverse](https://github.com/g3rv4/GetMoarFediverse).

Other people have built other things on top of FakeRelay:

* Abhinav Sarkar wrote [an article](https://notes.abhinavsarkar.net/2022/fake-relay) showing how you can achieve something similar to GetMoarFediverse in Python
* Raynor built [Fake Firehose](https://github.com/raynormast/fake-firehose), a tool that streams content from other instances and pushes that to FakeRelay.

## How can I use it?

### You need to get an api key

Ask the operator for an api key. If you want an api key for fakerelay.gervas.io, I'm `@g3rv4@mastodonte.tech`. Send me a toot with your instance domain and I'll get you one. The API key will be associated with your domain.

I'm hosting this behind Cloudflare Workers. So if you have a lot of traffic, I'll ask you to run it on your infrastructure :)

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

## Can I see a demo?

Sure! I recorded one [here](https://youtu.be/ungRlYKHS0E).

## I want to run this myself!

Sure thing! The easiest way is doing so via `docker-compose`. One important thing to notice is that all the instances that use your FakeRelay will treat it as a real relay. That means they will send traffic your way.

FakeRelay ignores this traffic, but that will still count against your badwidth. What I did is I set up Cloudflare so that it only forwards `POST`s to `/inbox` if the `type` is `Follow`.

### Setting up docker compose

I'm using this `docker-compose.yml`:

```
version: '2'
services:
  fakerelay:
    image: 'ghcr.io/g3rv4/fakerelay:latest'
    command: web
    hostname: fakerelay
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
      - CONFIG_PATH=/data/config.json
    restart: always
    volumes:
      - '/local/path/to/data:/data'
    ports:
      - 5000:5000
  cli:
    image: 'ghcr.io/g3rv4/fakerelay:latest'
    volumes:
      - '/local/path/to/data:/data'
```

That will store the configuration files at `/local/path/to/data` (they are a couple json files).

### Setup SSL reverse proxy
The relay needs to be accessible via a domain name with https. The subdomain can be served via a reverse proxy that also handles the SSL encryption. 

#### Nginx config file
```
server {
  listen 443 ssl http2;
  listen [::]:443 http2 ssl;

  # Uncomment and change these lines if you want to restrict access to fakerelay
  # allow Your-Instance-IPv6-address;
  # allow Your-Instance-IPv4-address;
  # allow GetMoarFediverse-Container-IP;
  # deny all;

  server_name relay.domain.tld;

  location / {
    proxy_pass http://127.0.0.1:5000;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header Host $host;
    proxy_max_temp_file_size 0;
  }

    #ssl_certificate /etc/letsencrypt/live/relay.domain.tld/fullchain.pem; # managed by Certbot
    #ssl_certificate_key /etc/letsencrypt/live/relay.domain.tld/privkey.pem; # managed by Certbot
}
```

### Configure the app

The first time you run this it needs to create a key, you can trigger that using:

```
docker-compose run --rm cli config {relayHost}
```

If you want requests to the homepage to redirect visitors somewhere, you can add a `"HomeRedirect"` entry on the generated `config.json`. The file would look like this:

```
{
    "PublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
    "PrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
    "Host": "fakerelay.gervas.io",
    "HomeRedirect": "https://github.com/g3rv4/FakeRelay/"
}
```

### List authorized instances
```
g3rv4@s1:~/docker/FakeRelay$ docker-compose run --rm cli list-instances
┌─────────────────┬──────────────────────────────────────────────────────────────────────────────────────────┐
│ Instance        │ Key                                                                                      │
├─────────────────┼──────────────────────────────────────────────────────────────────────────────────────────┤
│ m2.g3rv4.com    │ KlYKnm9GJcM0B1p8K98vw8FSpWzWOimZ7/3C9kTdWGUmK3xmFEJJwTZ1wqERVTugLH/9alYILFehqu9Ns2MEAw== │
│ mastodon.social │ 1TxL6m1Esx6tnv4EPxscvAmdQN7qSn0nKeyoM7LD8b9mz+GNfrKaHiWgiT3QcNMUA+dWLyWD8qyl1MuKJ+4uHA== │
└─────────────────┴──────────────────────────────────────────────────────────────────────────────────────────┘
```

### Add authorized instance

When you add an instance, the system will generate a token to index stuff on it and return that:

```
g3rv4@s1:~/docker/FakeRelay$ docker-compose run --rm cli instance add mastodon.social
Key generated for mastodon.social
vti7J0MDDw1O5EPRwfuUafJJjpErhXTwECGEvuw/G4UVWgLXtnrnmPIRRsOcvMD0juwSlvUnchIzgla030AIRw==
```

### Rotate a key

You can use `instance update` to rotate a instance's key:

```
g3rv4@s1:~/docker/FakeRelay$ docker-compose run --rm cli instance update mastodon.social
Key generated for mastodon.social
wpSX9xpPgX0gjgAxO0Jc+GLSOXubVgv73FOvAihR2EmgK/AfDHz21sF72uqrLnVGzcq2BDXosMeKdFR76q6fpg==
```

### Remove an instance

If you want to revoke a instance's key, you can use `instance delete`:

```
g3rv4@s1:~/docker/FakeRelay$ docker-compose run --rm cli instance delete mastodon.social
Key deleted for mastodon.social
```
