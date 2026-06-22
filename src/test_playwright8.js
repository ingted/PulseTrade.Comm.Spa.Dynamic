
const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  await page.goto("http://127.0.0.1:6137/chat", { waitUntil: "networkidle" });
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
    
    const html = await page.content();
    require("fs").writeFileSync("chat_showcase.html", html);
  }
  await browser.close();
})();

