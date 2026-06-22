
const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  page.on("console", msg => console.log("BROWSER CONSOLE:", msg.type(), msg.text()));
  page.on("pageerror", err => console.log("BROWSER ERROR:", err.message));
  page.on("requestfailed", request => console.log("FAILED RESOURCE:", request.url(), request.failure().errorText));

  console.log("Navigating to chat page...");
  await page.goto("http://127.0.0.1:6137/chat", { waitUntil: "networkidle" });
  await page.waitForTimeout(1000);
  
  // Try to find the Showcase Actor in the sidebar
  const showcaseActor = page.locator("[data-participant-id=\"agent.showcase\"]");
  if (await showcaseActor.count() > 0) {
    console.log("Found Showcase Actor, clicking...");
    await showcaseActor.click();
    await page.waitForTimeout(1000);
    
    // Type and send a message
    const draft = page.locator("[data-testid=\"chat-draft\"]");
    await draft.fill("init");
    const sendBtn = page.locator("[data-testid=\"chat-send\"]");
    await sendBtn.click();
    console.log("Sent message to Showcase Actor.");
    
    await page.waitForTimeout(2000);
    
    // Click on the summary card to expand it (if it exists)
    const summaryCard = page.locator(".sdui-json-snippet");
    if (await summaryCard.count() > 0) {
      console.log("Found summary card, clicking to expand JSON...");
      await summaryCard.click();
      await page.waitForTimeout(1000);
    }
    
    // Check if Canvas Components are present
    const headings = page.locator("h2");
    console.log("Found h2 count:", await headings.count());

    await page.screenshot({ path: "chat_result_showcase.png" });
  } else {
    console.log("Could not find Showcase actor in chat sidebar.");
    await page.screenshot({ path: "chat_result_no_actor.png" });
  }

  await browser.close();
})();

