
const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  console.log("Navigating to chat page...");
  await page.goto("http://127.0.0.1:4431/chat", { waitUntil: "networkidle" });
  await page.waitForTimeout(1000);
  
  // We need to add showcase-dynamic-actor to the participant list!
  // Wait, there is a "Participants" or we can send via console?
  // Let us just evaluate JS to send a message via the websocket.
  // Actually, we can just click "Sets" -> Add Participant ? No.
  // The easiest way is to modify poc.dynamic.fsx to automatically send a message to showcase-dynamic-actor on behalf of the user,
  // OR register showcase-dynamic-actor as a participant so it shows up in the sidebar!
  
  await browser.close();
})();

