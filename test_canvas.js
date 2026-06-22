const { chromium } = require('playwright');
const fs = require('fs');

(async () => {
    console.log("Starting browser...");
    // Using headless: false to keep the browser visible for the user
    const browser = await chromium.launch({ headless: false });
    // We do NOT close the browser context/page so it stays open.
    const context = await browser.newContext();
    const page = await context.newPage();
    
    try {
        page.on('response', response => {
            if (response.status() >= 400) {
                console.log(`BROWSER ERROR STATUS ${response.status()}: ${response.url()}`);
            }
        });
        page.on('console', msg => {
            console.log('BROWSER CONSOLE:', msg.text());
            if (msg.location() && msg.location().url) {
                console.log('CONSOLE MSG URL:', msg.location().url);
            }
        });
        
        page.on('response', async response => {
            if (response.url().includes('/pages/api/')) {
                console.log('API Response:', response.url(), response.status());
                try {
                    const json = await response.json();
                    if (response.url().includes('state')) {
                        console.log('API Response JSON:', JSON.stringify(json, null, 2));
                    } else {
                        console.log('API Response JSON:', JSON.stringify(json).substring(0, 500));
                    }
                } catch (e) {}
            }
        });

        page.on('websocket', ws => {
            ws.on('framereceived', frame => {
                console.log('WS RECEIVE:', frame.payload.toString().substring(0, 500));
            });
            ws.on('framesent', frame => {
                console.log('WS SEND:', frame.payload.toString().substring(0, 500));
            });
        });
        page.on('pageerror', err => console.log('BROWSER ERROR:', err));
        page.on('requestfailed', request => console.log('REQUEST FAILED:', request.url(), request.failure().errorText));
        page.on('response', response => {
            if (!response.ok()) console.log('RESPONSE NOT OK:', response.url(), response.status());
        });

        console.log("Navigating to chat page...");
        await page.goto('http://127.0.0.1:2676/actors', { timeout: 10000 }).catch(e => console.log("Goto timeout, but continuing..."));
        
        await page.waitForSelector('[data-testid="page-create"]');
        
        console.log("Creating new Actor Dynamic page...");
        await page.click('[data-testid="page-create"]');
        await page.waitForTimeout(500);
        const uniqueId = "canvas-test-" + Date.now();
        await page.selectOption('[data-testid="page-create-shape"]', 'actor-dynamic');
        await page.fill('[data-testid="page-create-title"]', 'Canvas Test ' + uniqueId);
        await page.fill('[data-testid="page-create-id"]', uniqueId);
        await page.click('[data-testid="page-create-submit"]');
        await page.waitForTimeout(1000);
        
        console.log("Clicking the new page...");
        await page.click(`text=Canvas Test ${uniqueId}`);
        await page.waitForTimeout(1000);
        
        console.log("Typing actor key...");
        await page.fill('[data-testid="append-key-input"]', '"akka.tcp://PulseTradeCommSpaDynamicPoc@127.0.0.1:2685/user/showcase-dynamic-actor"');
        await page.click('[data-testid="append-add-key"]');
        
        console.log("Waiting for key card...");
        try {
            await page.waitForSelector('[data-testid="append-key-card"]', { state: 'visible', timeout: 5000 });
            await page.click('[data-testid="append-key-card"]');
            await page.waitForTimeout(1000);
            
            console.log("Typing message...");
            const sduiPayload = `{"schema":"fskynet-sdui","ui":[{"id":"demo-canvas","text":"PulseTrade Actor Dynamic Dashboard","type":"Heading"}]}`;
            await page.screenshot({ path: 'canvas_before_send.png' });
            
            await page.fill('[data-testid="append-value-input"]', sduiPayload);
            await page.screenshot({ path: 'canvas_after_fill.png' });
            
            console.log("Clicking send...");
            await page.click('[data-testid="append-submit"]');
            
            console.log("Waiting 5 seconds to observe the result...");
            await page.waitForTimeout(5000);
        } catch (e) {
            console.log("Error waiting for key card:", e.message);
        } 
        
        console.log("Checking for canvas...");
        const html = await page.content();
        if (html.includes('FSkynet SDUI Canvas') || html.includes('FSkynet')) {
            console.log("SUCCESS: Canvas found in HTML!");
        } else {
            console.log("FAILURE: Canvas not found.");
            fs.writeFileSync('canvas_debug.html', html);
        }
        
        await page.screenshot({ path: 'canvas_test_result.png' });
        console.log("Screenshot saved. Browser is left open for the user.");
        // Do not call browser.close()
    } catch (e) {
        console.error("Error:", e);
        // Only close on error
        await browser.close();
    }
})();

