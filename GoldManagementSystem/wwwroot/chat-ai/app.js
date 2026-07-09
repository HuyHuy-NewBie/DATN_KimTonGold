// Available Store Branches list
const storeBranches = [
    {
        id: "badinh",
        name: "KIMTON Ba Đình (Trụ sở chính)",
        hotline: "0961137407",
        openHours: "08:00 - 21:30 hàng ngày",
        address: "Số 123 Đường Kim Mã, Quận Ba Đình, Hà Nội"
    },
    {
        id: "caugiay",
        name: "KIMTON Cầu Giấy",
        hotline: "0961137408",
        openHours: "08:30 - 22:00 hàng ngày",
        address: "Số 456 Đường Cầu Giấy, Quận Cầu Giấy, Hà Nội"
    },
    {
        id: "quan1",
        name: "KIMTON Quận 1 HCM",
        hotline: "0961137409",
        openHours: "09:00 - 22:00 hàng ngày",
        address: "Số 789 Đường Nguyễn Huệ, Bến Nghé, Quận 1, TP. Hồ Chí Minh"
    }
];

// Global State containing the Gold Shop Configuration (initialized to Ba Dinh main branch)
let storeConfig = {
    name: storeBranches[0].name,
    hotline: storeBranches[0].hotline,
    openHours: storeBranches[0].openHours,
    address: storeBranches[0].address,
    exchangePolicy: "Mua lại 75% giá trị hóa đơn, đổi mẫu mới thu hồi 80%",
    warrantyPolicy: "Đánh bóng trọn đời, miễn phí đính lại đá tấm rớt trong 6 tháng",
    prices: {
        gold24k: { buy: 7450000, sell: 7600000 },
        gold18k: { buy: 5350000, sell: 5550000 },
        gold14k: { buy: 4100000, sell: 4300000 }
    }
};

// Vietnamese Ring Size Standard Mapping Table (circumference in mm to VN size)
const ringSizeMap = [
    { circum: 44, size: 4 },
    { circum: 45, size: 5 },
    { circum: 46, size: 6 },
    { circum: 47, size: 7 },
    { circum: 48, size: 8 },
    { circum: 49, size: 9 },
    { circum: 50, size: 10 },
    { circum: 51, size: 11 },
    { circum: 52, size: 12 },
    { circum: 53, size: 13 },
    { circum: 54, size: 14 },
    { circum: 55, size: 15 },
    { circum: 56, size: 16 },
    { circum: 57, size: 17 },
    { circum: 58, size: 18 },
    { circum: 59, size: 19 },
    { circum: 60, size: 20 },
    { circum: 61, size: 21 },
    { circum: 62, size: 22 },
    { circum: 63, size: 23 },
    { circum: 64, size: 24 },
    { circum: 65, size: 25 },
    { circum: 66, size: 26 },
    { circum: 67, size: 27 },
    { circum: 68, size: 28 },
    { circum: 69, size: 29 },
    { circum: 70, size: 30 }
];

// DOM Elements
const chatMessages = document.getElementById('chatMessages');
const userInput = document.getElementById('userInput');
const sendBtn = document.getElementById('sendBtn');
const quickChips = document.querySelector('.quick-chips');
const goldPriceTableBody = document.getElementById('goldPriceTableBody');
const ringSizeOutput = document.getElementById('ringSizeOutput');
const calcResult = document.getElementById('calcResult');
const circumferenceInput = document.getElementById('circumference');
const btnCalc = document.getElementById('btnCalc');
const btnExport = document.getElementById('btnExport');
const promptModal = document.getElementById('promptModal');
const modalClose = document.getElementById('modalClose');
const promptCode = document.getElementById('promptCode');
const btnCopyPrompt = document.getElementById('btnCopyPrompt');
const toast = document.getElementById('toast');

// Display Elements
const displayStoreName = document.getElementById('displayStoreName');
const displayHotline = document.getElementById('displayHotline');
const displayOpenHours = document.getElementById('displayOpenHours');
const displayAddress = document.getElementById('displayAddress');
const displayExchangePolicy = document.getElementById('displayExchangePolicy');
const displayWarrantyPolicy = document.getElementById('displayWarrantyPolicy');

// Initialize Web App
document.addEventListener('DOMContentLoaded', () => {
    populateBranchSelector();
    renderStoreInfo();
    renderPriceBoard();
    initEventListeners();
});

// Render Store Information
function renderStoreInfo() {
    displayStoreName.textContent = storeConfig.name;
    displayHotline.textContent = storeConfig.hotline;
    displayOpenHours.textContent = storeConfig.openHours;
    displayAddress.textContent = storeConfig.address;
    displayExchangePolicy.textContent = storeConfig.exchangePolicy;
    displayWarrantyPolicy.textContent = storeConfig.warrantyPolicy;
    
    // Sync chat welcoming names
    document.querySelectorAll('.ai-sync-store-name').forEach(el => {
        el.textContent = storeConfig.name;
    });
}

// Populate Branch Select Options and hook change listener
function populateBranchSelector() {
    const branchSelector = document.getElementById('branchSelector');
    if (!branchSelector) return;
    
    let html = '';
    storeBranches.forEach(branch => {
        html += `<option value="${branch.id}">${branch.name}</option>`;
    });
    branchSelector.innerHTML = html;

    branchSelector.addEventListener('change', (e) => {
        const selectedId = e.target.value;
        const branch = storeBranches.find(b => b.id === selectedId);
        if (branch) {
            storeConfig.name = branch.name;
            storeConfig.hotline = branch.hotline;
            storeConfig.openHours = branch.openHours;
            storeConfig.address = branch.address;
            
            // Re-render display
            renderStoreInfo();
            renderPriceBoard();
            
            // Update dynamically rendered title on chat
            document.getElementById('chatAssistantTitle').textContent = `Trợ Lý ${storeConfig.name.replace(" (Trụ sở chính)", "").replace(" HCM", "")}`;
            
            // Feedback
            showToast(`Đã chuyển sang ${branch.name}`);
            addSystemMessage(`Hệ thống đã kết nối với Chi nhánh ${branch.name}.`);
        }
    });
}

// Init Event Listeners
function initEventListeners() {
    // Chat events
    sendBtn.addEventListener('click', handleSendMessage);
    userInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') handleSendMessage();
    });

    // Quick reply chips
    quickChips.addEventListener('click', (e) => {
        if (e.target.classList.contains('chip')) {
            const question = e.target.getAttribute('data-question');
            sendUserMessage(question);
            triggerBotResponse(question);
        }
    });

    // Calculator event
    btnCalc.addEventListener('click', calculateRingSize);

    // Export system prompt modal events
    if (btnExport) btnExport.addEventListener('click', showPromptModal);
    if (modalClose) modalClose.addEventListener('click', closePromptModal);
    if (promptModal) {
        promptModal.addEventListener('click', (e) => {
            if (e.target === promptModal) closePromptModal();
        });
    }

    // Copy prompt
    if (btnCopyPrompt) btnCopyPrompt.addEventListener('click', copySystemPrompt);
}

// Render Price Board
function renderPriceBoard() {
    const list = [
        { name: "Vàng SJC / 24K (9999)", key: 'gold24k' },
        { name: "Vàng Tây 18K (Trang sức)", key: 'gold18k' },
        { name: "Vàng Tây 14K (Trang sức)", key: 'gold14k' }
    ];

    let html = '';
    list.forEach(item => {
        const buyVal = storeConfig.prices[item.key].buy.toLocaleString('vi-VN');
        const sellVal = storeConfig.prices[item.key].sell.toLocaleString('vi-VN');
        html += `
            <tr>
                <td><strong>${item.name}</strong></td>
                <td class="up">${buyVal} đ</td>
                <td class="down">${sellVal} đ</td>
            </tr>
        `;
    });
    
    const container = document.getElementById('goldPriceTableBody');
    if (container) {
        container.innerHTML = html;
    }
}

// Update Config state from Form
function updateConfigState() {
    storeConfig.name = document.getElementById('storeName').value;
    storeConfig.hotline = document.getElementById('hotline').value;
    storeConfig.openHours = document.getElementById('openHours').value;
    storeConfig.address = document.getElementById('storeAddress').value;
    storeConfig.exchangePolicy = document.getElementById('exchangePolicy').value;
    storeConfig.warrantyPolicy = document.getElementById('warrantyPolicy').value;

    storeConfig.prices.gold24k.buy = parseInt(document.getElementById('gold24kBuy').value) || 0;
    storeConfig.prices.gold24k.sell = parseInt(document.getElementById('gold24kSell').value) || 0;
    storeConfig.prices.gold18k.buy = parseInt(document.getElementById('gold18kBuy').value) || 0;
    storeConfig.prices.gold18k.sell = parseInt(document.getElementById('gold18kSell').value) || 0;
    storeConfig.prices.gold14k.buy = parseInt(document.getElementById('gold14kBuy').value) || 0;
    storeConfig.prices.gold14k.sell = parseInt(document.getElementById('gold14kSell').value) || 0;

    // Update dynamically rendered title on chat and price tables
    document.getElementById('chatAssistantTitle').textContent = `Trợ Lý ${storeConfig.name.replace("Tiệm Vàng ", "")}`;
    document.querySelectorAll('.ai-sync-store-name').forEach(el => {
        el.textContent = storeConfig.name;
    });

    renderPriceBoard();
}

// Show Toast Alert
function showToast(message) {
    toast.textContent = message;
    toast.classList.add('show');
    setTimeout(() => {
        toast.classList.remove('show');
    }, 2500);
}

// Chat functions
function handleSendMessage() {
    const text = userInput.value.trim();
    if (!text) return;

    sendUserMessage(text);
    userInput.value = '';
    triggerBotResponse(text);
}

// Escapes special characters for safe output injection
function escapeHTML(str) {
    return str.replace(/[&<>'"]/g, 
        tag => ({
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            "'": '&#39;',
            '"': '&quot;'
        }[tag] || tag)
    );
}

function sendUserMessage(text) {
    const time = getFormattedTime();
    const msgHtml = `
        <div class="message user">
            <div class="msg-bubble">${escapeHTML(text)}</div>
            <span class="msg-time">${time}</span>
        </div>
    `;
    chatMessages.insertAdjacentHTML('beforeend', msgHtml);
    scrollToBottom();
}

// Render server/system update messages
function addSystemMessage(text) {
    const msgHtml = `
        <div class="message system-msg">
            <span>${escapeHTML(text)}</span>
        </div>
    `;
    chatMessages.insertAdjacentHTML('beforeend', msgHtml);
    scrollToBottom();
}

// Simulate AI responses
function triggerBotResponse(userMsg) {
    // Add typing indicator
    const typingId = 'typing-' + Date.now();
    const typingHtml = `
        <div class="message assistant" id="${typingId}">
            <div class="msg-bubble">
                <div class="typing-indicator">
                    <span class="typing-dot"></span>
                    <span class="typing-dot"></span>
                    <span class="typing-dot"></span>
                </div>
            </div>
        </div>
    `;
    chatMessages.insertAdjacentHTML('beforeend', typingHtml);
    scrollToBottom();

    // Generate response delay
    setTimeout(() => {
        const indicator = document.getElementById(typingId);
        if (indicator) indicator.remove();

        const reply = generateAIReply(userMsg);
        const time = getFormattedTime();
        const replyHtml = `
            <div class="message assistant">
                <div class="msg-bubble">${reply}</div>
                <span class="msg-time">${time}</span>
            </div>
        `;
        chatMessages.insertAdjacentHTML('beforeend', replyHtml);
        scrollToBottom();
    }, 900);
}

// NLP simulated keyword matcher and generator
function generateAIReply(msg) {
    const text = msg.toLowerCase();
    const name = storeConfig.name;
    const phone = storeConfig.hotline;
    const hours = storeConfig.openHours;
    const addr = storeConfig.address;
    
    // 1. Gold Prices (Important Rule: No static fake prices in text, redirect to hotline)
    if (text.includes("giá") || text.includes("gia") || text.includes("bao nhieu một chỉ") || text.includes("bao nhieu 1 chi")) {
        return `Dạ, giá vàng biến động liên tục theo từng giờ trên thị trường. Để nhận báo giá chính xác nhất ở thời điểm hiện tại kèm theo tiền công chế tác và ưu đãi mới nhất, anh/chị vui lòng nhắn tin trực tiếp số Hotline/Zalo <strong>${phone}</strong> hoặc để lại số điện thoại, nhân viên của cửa hàng sẽ liên hệ báo giá ngay lập tức ạ.`;
    }

    // 2. Size / Ring size / Measurement
    if (text.includes("size") || text.includes("đo") || text.includes("do tay") || text.includes("size tay")) {
        return `Dạ, để tự đo size tay chuẩn tại nhà nhằm chọn nhẫn chính xác nhất, anh/chị có thể quấn một sợi chỉ hoặc dải giấy nhỏ ôm sát quanh ngón tay cần đeo, đánh dấu điểm giao nhau và đo chiều dài của dải giấy đó (đây là chu vi ngón tay, tính bằng mm). 
        <br><br>
        Sau đó anh/chị có thể nhập số đo mm đó vào <strong>công cụ "Tính Size Nhẫn Tương Tác"</strong> ở cột bên phải màn hình để nhận size nhẫn gợi ý ngay lập tức, hoặc báo lại cho em số đo để em tra size giúp mình nhé ạ!`;
    }

    // 3. Store Address, closing/opening hours
    if (text.includes("địa chỉ") || text.includes("dia chi") || text.includes("ở đâu") || text.includes("cua hang") || text.includes("chi nhánh")) {
        return `Dạ, địa chỉ cửa hàng vàng bạc đá quý <strong>${name}</strong> hiện tại ở: 
        <br>📍 <em>${addr}</em> 
        <br>Cửa hàng mở cửa từ <strong>${hours}</strong> hàng ngày để phục vụ quý khách. Rất mong được đón tiếp anh/chị ghé qua tham quan ạ!`;
    }

    // 4. Contact / Hotline
    if (text.includes("hotline") || text.includes("sđt") || text.includes("sdt") || text.includes("số điện thoại") || text.includes("liên hệ") || text.includes("zalo")) {
        return `Dạ, anh/chị có thể liên hệ trực tiếp với chuyên viên tư vấn của <strong>${name}</strong> qua số điện thoại/Zalo: <strong>${phone}</strong> để được phản hồi và tư vấn nhanh nhất ạ.`;
    }

    // 5. Exchange / buyback policy
    if (text.includes("đổi") || text.includes("doi") || text.includes("thu mua") || text.includes("bán lại") || text.includes("chính sách thu")) {
        return `Dạ, chính sách thu đổi và bảo hành của trang sức tại <strong>${name}</strong> như sau ạ:
        <br><br>
        <strong>1. Về thu đổi trang sức vàng tây:</strong>
        <br>👉 ${storeConfig.exchangePolicy}.
        <br><br>
        <strong>2. Về thu đổi vàng ta (24K/9999):</strong>
        <br>👉 Tiệm mua vào/bán ra theo đúng bảng giá vàng miếng niêm yết của tiệm tại đúng thời điểm giao dịch thực tế.`;
    }

    // 6. Warranty policy
    if (text.includes("bảo hành") || text.includes("bao hanh") || text.includes("đánh bóng") || text.includes("danh bong") || text.includes("rớt đá") || text.includes("hàn")) {
        return `Dạ, đối với các sản phẩm trang sức mua tại <strong>${name}</strong>, quý khách sẽ được hưởng chế độ hậu mãi đặc biệt:
        <br>👉 ${storeConfig.warrantyPolicy}.
        <br>Anh/chị có thể mang trang sức ghé qua cửa hàng bất kỳ lúc nào để nhân viên kỹ thuật hỗ trợ chăm sóc làm mới hoàn toàn miễn phí ạ!`;
    }

    // 7. Custom Ring Design / Make customized jewelry
    if (text.includes("thiết kế") || text.includes("thiet ke") || text.includes("làm riêng") || text.includes("lam rieng") || text.includes("đặt mẫu")) {
        return `Dạ, tiệm nhận đặt thiết kế riêng trang sức vàng và nhẫn cưới/nhẫn thời trang theo yêu cầu của anh/chị.
        <br><br>
        Anh/chị vui lòng để lại thông tin <strong>Tên, Số điện thoại và mô tả sơ bộ sản phẩm quan tâm</strong>. Chuyên viên thiết kế trang sức bên em sẽ lập tức liên hệ gửi mẫu vẽ 3D và báo giá gia công chi tiết qua Zalo cho mình ạ!`;
    }

    // 8. General greetings
    if (text.includes("chào") || text.includes("hello") || text.includes("hi") || text.includes("cửa hàng")) {
        return `Dạ, chuyên viên tư vấn tiệm vàng <strong>${name}</strong> xin chào anh/chị! Em có thể giúp gì cho anh/chị hôm nay ạ?`;
    }

    // 9. Standard thank you response
    if (text.includes("cảm ơn") || text.includes("cam on") || text.includes("thank")) {
        return `Dạ không có gì ạ! Rất hy vọng được phục vụ anh/chị tại <strong>${name}</strong> trong thời gian tới. Nếu cần thêm thông tin gì khác, anh/chị cứ nhắn em nhé ạ! Chúc anh/chị một ngày vui vẻ!`;
    }

    // 10. Fallback response (looks like a custom request/question)
    return `Dạ, em đã ghi nhận thông tin yêu cầu của anh/chị về sản phẩm này. Để tư vấn sâu hơn và gửi hình ảnh thực tế hoặc các catalogue mẫu trang sức phù hợp, anh/chị vui lòng cung cấp thêm số điện thoại hoặc nhắn tin trực tiếp qua Hotline/Zalo <strong>${phone}</strong> để chuyên viên hỗ trợ anh/chị chu đáo nhất ạ!`;
}

// Calculate Ring size from circumference
// Map logic
function calculateRingSize() {
    const val = parseFloat(circumferenceInput.value);
    if (isNaN(val) || val < 30 || val > 100) {
        showToast("Vui lòng nhập chu vi từ 30 đến 100 mm!");
        return;
    }

    // Find closest match in the map
    let closest = ringSizeMap[0];
    let minDiff = Math.abs(val - closest.circum);

    for (let i = 1; i < ringSizeMap.length; i++) {
        const diff = Math.abs(val - ringSizeMap[i].circum);
        if (diff < minDiff) {
            minDiff = diff;
            closest = ringSizeMap[i];
        }
    }

    ringSizeOutput.textContent = `Size ${closest.size}`;
    calcResult.style.display = 'block';
    
    // Add dynamic glow to the result box
    const box = document.querySelector('.result-box');
    box.style.boxShadow = '0 0 15px rgba(212, 175, 55, 0.4)';
    setTimeout(() => {
        box.style.boxShadow = 'none';
    }, 1000);
}

// Modal show / close prompt exporter
function showPromptModal() {
    const promptText = generateSystemPromptContent();
    promptCode.textContent = promptText;
    promptModal.classList.add('show');
}

function closePromptModal() {
    promptModal.classList.remove('show');
}

// Copy prompt logic
function copySystemPrompt() {
    const promptText = promptCode.textContent;
    navigator.clipboard.writeText(promptText)
        .then(() => {
            showToast("Đã copy System Prompt vào Clipboard!");
            closePromptModal();
        })
        .catch(err => {
            showToast("Lỗi sao chép! Vui lòng copy thủ công.");
            console.error('Không thể sao chép văn bản: ', err);
        });
}

// Generate the custom Markdown Prompt string based on current state
function generateSystemPromptContent() {
    const pricesText = `
  + Vàng SJC / 24K (9999): Mua vào ${storeConfig.prices.gold24k.buy.toLocaleString('vi-VN')} đ/chỉ - Bán ra ${storeConfig.prices.gold24k.sell.toLocaleString('vi-VN')} đ/chỉ
  + Vàng Tây 18K: Mua vào ${storeConfig.prices.gold18k.buy.toLocaleString('vi-VN')} đ/chỉ - Bán ra ${storeConfig.prices.gold18k.sell.toLocaleString('vi-VN')} đ/chỉ
  + Vàng Tây 14K: Mua vào ${storeConfig.prices.gold14k.buy.toLocaleString('vi-VN')} đ/chỉ - Bán ra ${storeConfig.prices.gold14k.sell.toLocaleString('vi-VN')} đ/chỉ`;

    return `Bạn là Trợ Lý KIMTON — chuyên viên tư vấn cao cấp của tiệm vàng ${storeConfig.name}.

## THÔNG TIN CỬA HÀNG (luôn dùng chính xác dữ liệu dưới đây, không tự bịa)
- Tên: ${storeConfig.name}
- Hotline/Zalo: ${storeConfig.hotline}
- Giờ mở cửa: ${storeConfig.openHours}
- Địa chỉ: ${storeConfig.address}
- Bảng giá vàng hôm nay: ${pricesText}
- Giá công chế tác: Dao động từ 300.000 đ đến 1.500.000 đ tùy thuộc vào độ tinh xảo và khối lượng của từng sản phẩm.
- Chính sách thu đổi: ${storeConfig.exchangePolicy}
- Chính sách đặt thiết kế riêng: Nhận đặt chế tác theo yêu cầu cá nhân (3D Render miễn phí). Thời gian chế tác hoàn thiện từ 7-10 ngày làm việc sau khi chốt thiết kế.
- Danh mục sản phẩm: Nhẫn cưới, Nhẫn cầu hôn, Nhẫn nam/nữ thời trang, Bông tai/Hoa tai, Dây chuyền, Lắc tay/Vòng tay, Mặt dây chuyền các loại chất liệu Vàng 24K, Vàng 18K, Vàng 14K.

## NGUYÊN TẮC TRẢ LỜI

1. TRẢ LỜI THẲNG, KHÔNG NÉ TRÁNH
Nếu câu hỏi có thể trả lời bằng dữ liệu ở trên, luôn trả lời trực tiếp và cụ thể (số liệu, chính sách...). Tuyệt đối không đẩy khách qua hotline nếu thông tin đã có sẵn.
Chỉ đẩy qua hotline/Zalo khi: hỏi giá công cho mẫu thiết kế riêng chưa có sẵn, hỏi vấn đề cần thẩm định trực tiếp (kiểm định vàng, khiếu nại, đơn hàng giá trị lớn), hoặc câu hỏi ngoài phạm vi dữ liệu bạn có.

2. LUÔN DẪN DẮT SANG BƯỚC TIẾP THEO
Sau khi trả lời, luôn kết bằng một câu hỏi mở hoặc gợi ý hành động cụ thể để giữ mạch tư vấn, phù hợp với câu hỏi vừa trả lời. Không trả lời cụt rồi dừng.

3. PHÂN LOẠI Ý ĐỊNH KHÁCH HÀNG
- Hỏi tham khảo chung (giá vàng, thông tin cơ bản): trả lời gọn, nhẹ nhàng, không dồn ép mua.
- Hỏi cụ thể (mẫu, size, ngân sách, số lượng): đây là tín hiệu mua thật — tư vấn sâu hơn, gợi ý sản phẩm phù hợp, hỏi thêm nhu cầu.
- Khiếu nại/phàn nàn: xin lỗi ngắn gọn, chuyển ngay qua hotline, không tự phán xét hay hứa hẹn thay cửa hàng.
- Đơn hàng giá trị lớn (vàng miếng đầu tư, bộ trang sức cưới...): tư vấn sơ bộ rồi chủ động đề nghị kết nối nhân viên thật để đảm bảo độ chính xác và tin cậy.

4. GIỌNG VĂN
Xưng "em", gọi khách "anh/chị". Lịch sự, ấm áp, chuyên nghiệp như nhân viên tư vấn giỏi tại cửa hàng — không máy móc, không lặp lại nguyên văn một mẫu câu cho mọi tình huống.

5. KHÔNG TỰ BỊA THÔNG TIN
Không tự đưa ra số liệu, chính sách, hay cam kết nào không có trong dữ liệu cửa hàng cung cấp. Nếu không chắc, nói rõ sẽ cần xác nhận qua hotline.

6. KẾT THÚC BẰNG HÀNH ĐỘNG RÕ RÀNG
Ưu tiên chốt câu trả lời bằng lời mời cụ thể: để lại số điện thoại, ghé cửa hàng, hoặc câu hỏi giúp xác định nhu cầu tiếp theo — thay vì câu chung chung "vui lòng liên hệ".`;
}

// Utility Helper functions
function getFormattedTime() {
    const now = new Date();
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes}`;
}

function scrollToBottom() {
    chatMessages.scrollTop = chatMessages.scrollHeight;
}
