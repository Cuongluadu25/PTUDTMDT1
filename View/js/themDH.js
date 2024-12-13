// Dữ liệu giả lập cho sản phẩm
const products = [
    { name: "Sách Toán", price: 50000 },
    { name: "Sách Văn", price: 60000 },
    { name: "Sách Anh", price: 70000 }
];

// Dữ liệu giả lập cho người dùng
const users = [
    { username: "userA", name: "Nguyễn Văn A", phone: "0901234567" },
    { username: "userB", name: "Trần Thị B", phone: "0902345678" },
    { username: "userC", name: "Lê Văn C", phone: "0903456789" }
];

// Lấy các phần tử từ DOM
const productNameInput = document.getElementById("productName");
const suggestionsList = document.getElementById("suggestions");
const priceInput = document.getElementById("price");
const quantityInput = document.getElementById("quantity");
const totalPriceInput = document.getElementById("totalPrice");

const customerNameInput = document.getElementById("customerName");
const usernameSuggestionsList = document.getElementById("usernameSuggestions");
const recipientNameInput = document.getElementById("recipientName");
const phoneInput = document.getElementById("phone");

// 1. Xử lý autocomplete sản phẩm
productNameInput.addEventListener("input", () => {
    const query = productNameInput.value.toLowerCase();
    suggestionsList.innerHTML = ""; // Xóa gợi ý cũ

    if (query) {
        products.forEach(product => {
            if (product.name.toLowerCase().includes(query)) {
                const suggestionItem = document.createElement("li");
                suggestionItem.classList.add("list-group-item");
                suggestionItem.textContent = product.name;
                suggestionItem.addEventListener("click", () => {
                    productNameInput.value = product.name;
                    priceInput.value = product.price;
                    suggestionsList.innerHTML = ""; // Xóa danh sách sau khi chọn
                    updateTotalPrice(); // Cập nhật tổng giá tiền
                });
                suggestionsList.appendChild(suggestionItem);
            }
        });
    }
});

// 2. Cập nhật tổng giá tiền
quantityInput.addEventListener("input", () => {
    updateTotalPrice();
});

function updateTotalPrice() {
    const quantity = parseInt(quantityInput.value) || 0;
    const price = parseInt(priceInput.value) || 0;
    totalPriceInput.value = quantity * price;
}

// 3. Xử lý autocomplete cho username
customerNameInput.addEventListener("input", () => {
    const query = customerNameInput.value.toLowerCase();
    usernameSuggestionsList.innerHTML = ""; // Xóa gợi ý cũ

    if (query) {
        users.forEach(user => {
            if (user.username.toLowerCase().includes(query)) {
                const suggestionItem = document.createElement("li");
                suggestionItem.classList.add("list-group-item");
                suggestionItem.textContent = user.username;
                suggestionItem.addEventListener("click", () => {
                    customerNameInput.value = user.username;
                    recipientNameInput.value = user.name;
                    phoneInput.value = user.phone;
                    usernameSuggestionsList.innerHTML = ""; // Xóa danh sách sau khi chọn
                });
                usernameSuggestionsList.appendChild(suggestionItem);
            }
        });
    }
});

// 4. Xử lý sự kiện gửi form
document.querySelector("form").addEventListener("submit", (event) => {
    event.preventDefault(); // Ngăn chặn reload trang

    const orderData = {
        username: customerNameInput.value,
        recipientName: recipientNameInput.value,
        phone: phoneInput.value,
        productName: productNameInput.value,
        price: parseFloat(priceInput.value),
        quantity: parseInt(quantityInput.value),
        totalPrice: parseFloat(totalPriceInput.value),
        address: document.getElementById("address").value,
        notes: document.getElementById("notes").value,
        status: document.getElementById("status").value
    };

    // Kiểm tra dữ liệu đầu vào
    if (!orderData.username || !orderData.productName || !orderData.quantity || !orderData.address) {
        alert("Vui lòng điền đầy đủ thông tin!");
        return;
    }

    console.log("Dữ liệu đơn hàng:", orderData);
    alert("Đơn hàng đã được thêm thành công!");
    // Xóa form sau khi xử lý
    document.querySelector("form").reset();
});
