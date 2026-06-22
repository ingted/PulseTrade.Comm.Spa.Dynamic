const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage();
  await page.goto('http://127.0.0.1:2845/chat');
  await page.waitForTimeout(2000);
  
  const html = await page.content();
  const fs = require('fs');
  fs.writeFileSync('dom.html', html);
  
  await browser.close();
})();
