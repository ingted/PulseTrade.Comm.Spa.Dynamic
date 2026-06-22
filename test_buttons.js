const { chromium } = require('playwright');
const fs = require('fs');
(async () => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    await page.goto('http://127.0.0.1:10424/page/actor-argu-123321');
    await page.waitForTimeout(1000);
    await page.fill('[data-testid="append-key-input"]', 'akka.tcp');
    await page.waitForTimeout(1000);
    const buttons = await page.$$eval('button', els => els.map(e => e.outerHTML));
    console.log(buttons.join('\n'));
    await browser.close();
})();
