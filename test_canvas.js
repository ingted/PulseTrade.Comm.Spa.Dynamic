const { chromium } = require('playwright');
const fs = require('fs');

(async () => {
    console.log("Starting browser...");
    const browser = await chromium.launch({ headless: true });
    const page = await browser.newPage();
    
    try {
        console.log("Navigating to chat page...");
        await page.goto('http://127.0.0.1:6683/chat', { waitUntil: 'networkidle' });
        
        await page.waitForSelector('.ptc-side-item');
        
        console.log("Creating new Actor Argu page...");
        await page.click('#add-set-bucket');
        await page.waitForTimeout(500);
        
        await page.selectOption('#page-create-shape', 'actor-argu');
        await page.fill('#page-create-title', 'Canvas Test');
        await page.click('#page-create-submit');
        await page.waitForTimeout(1000);
        
        console.log("Clicking the new page...");
        await page.click('text=Canvas Test');
        await page.waitForTimeout(1000);
        
        console.log("Typing actor key...");
        await page.fill('input.fcell-input[placeholder="Actor Address (e.g. akka.tcp://...)"]', 'akka.tcp://PulseTradeCommSpaDynamicPoc@127.0.0.1:6683/user/showcase-dynamic-actor');
        
        console.log("Typing message...");
        const inputs = await page.$$('input.fcell-input');
        if (inputs.length >= 2) {
            await inputs[1].fill('test payload');
        } else {
            console.log("Not enough inputs found!");
        }
        
        console.log("Clicking send...");
        await page.click('button:has-text("Append")');
        await page.waitForTimeout(3000); 
        
        console.log("Checking for canvas...");
        const html = await page.content();
        if (html.includes('FSkynet SDUI Canvas') || html.includes('FSkynet 動態畫布')) {
            console.log("SUCCESS: Canvas found in HTML!");
        } else {
            console.log("FAILURE: Canvas not found.");
            fs.writeFileSync('canvas_debug.html', html);
        }
        
        await page.screenshot({ path: 'canvas_test_result.png' });
        console.log("Screenshot saved.");
    } catch (e) {
        console.error("Error:", e);
    } finally {
        await browser.close();
    }
})();
