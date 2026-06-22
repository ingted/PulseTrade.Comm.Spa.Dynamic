const { chromium } = require('playwright');
(async () => {
    const browser = await chromium.launch();
    const page = await browser.newPage();
    await page.goto('http://127.0.0.1:10424/page/actor-argu-123321');
    await page.waitForTimeout(2000);
    const inputs = await page.evaluate(() => {
        return Array.from(document.querySelectorAll('input, textarea, select')).map(e => e.outerHTML);
    });
    console.log(inputs.join('\n'));
    await browser.close();
})();
