# Our Online Shop (Explained Like You’re 10)

Imagine a toy store on the internet. You can look at toys, put your favorites in a basket, pay safely, and see your past orders. A shop owner can also go into a special room to add new toys, check what’s selling, and watch simple charts.

## What it does
- Look at products with pictures and prices
- Search and filter to find the right thing
- Add to cart and check out with a safe payment (Stripe)
- Make an account and log in (with a secret password)
- See your old orders
- Get suggestions for other things you might like
- The shop owner gets a dashboard with charts to see sales and popular pages

## How it’s built
- It speaks C# (a programming language) and runs on ASP.NET (a web engine)
- It saves data (like products and orders) in a database
- It uses math (ML.NET) to suggest products
- It shows pretty pictures and buttons with Bootstrap
- It keeps logs so we can fix problems

## Why it’s safe
- Your password is stored in a scrambled way so nobody can read it
- We use tokens (like special tickets) so only the right people get in
- We use HTTPS so snoops can’t eavesdrop
- We limit how fast people can knock on the door so bad bots can’t flood it

## How to try it
1. Start the website server.
2. Open the browser to the address it shows.
3. Make an account, log in, and explore.
4. Pretend to buy something using Stripe’s test mode.

## For the grown-ups
- API: ASP.NET Core Web API with Identity + JWT
- UI: Razor Pages + Chart.js for admin
- Data: EF Core (SQLite dev, SQL Server prod)
- Payments: Stripe
- Recs: ML.NET content-based similarity
- Infra: Docker + GitHub Actions

This project is like building your own tiny Shopify. It shows you can make a real app that’s safe, fast, and ready to ship.
