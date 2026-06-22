
const { chromium } = require("playwright");
const fs = require("fs");
(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage();
  const response = await page.goto("http://127.0.0.1:12179/chat");
  const html = await response.text();
  fs.writeFileSync("chat2.html", html);
  await browser.close();
})();

