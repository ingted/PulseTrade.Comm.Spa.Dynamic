
const { chromium } = require("playwright");
const fs = require("fs");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  await page.goto("http://127.0.0.1:4431/chat", { waitUntil: "networkidle" });
  fs.writeFileSync("chat_fixed.html", await page.content());

  await browser.close();
})();

