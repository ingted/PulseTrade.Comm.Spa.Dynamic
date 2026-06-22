const { chromium } = require('playwright');
(async () => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    await page.goto('http://127.0.0.1:10424/page/actor-dynamic-dddd');
    await page.waitForTimeout(2000);
    const html = await page.evaluate(() => document.body.innerHTML);
    console.log(html);
    await browser.close();
})();
