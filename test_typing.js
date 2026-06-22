const { chromium } = require('playwright');
(async () => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    await page.goto('http://127.0.0.1:10424/page/actor-argu-123321');
    await page.waitForTimeout(1000);
    await page.fill('[data-testid="append-key-input"]', 'akka.tcp');
    await page.waitForTimeout(1000);
    const html = await page.evaluate(() => document.body.innerHTML);
    const fs = require('fs');
    fs.writeFileSync('html_after_typing.html', html);
    await browser.close();
})();
