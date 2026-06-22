
const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  await page.goto("http://127.0.0.1:6681/chat", { waitUntil: "networkidle" });
  await page.waitForTimeout(1000);
  
  const showcaseActor = page.locator("[data-participant-id=\"agent.showcase\"]");
  if (await showcaseActor.count() > 0) {
    await showcaseActor.click();
    await page.waitForTimeout(1000);
    
    const draft = page.locator("[data-testid=\"chat-draft\"]");
    await draft.fill("init");
    const sendBtn = page.locator("[data-testid=\"chat-send\"]");
    await sendBtn.click();
    
    await page.waitForTimeout(2000);
    
    // click snippet
    const summaryCard = page.locator(".sdui-json-snippet");
    if (await summaryCard.count() > 0) {
      await summaryCard.click();
      await page.waitForTimeout(1000);
    }

    await page.screenshot({ path: "chat_result_showcase2.png" });
  }
  await browser.close();
})();

