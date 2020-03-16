var socket = new WebSocket("ws://127.0.0.1:47055");
let process = {
    Pid: 0,
    Name: 'string',
    User: 'string',
    Cpu: 0.0,
    Memory: 0.0,
    Time: 'string'
};
const formatterPercent = new Intl.NumberFormat('en-US', {
    style: 'decimal',
    minimumIntegerDigits: 1,
    minimumFractionDigits: 3
})
const formatterNumber = new Intl.NumberFormat('ru', {
    style: 'decimal',
    minimumFractionDigits: 0,
    useGrouping: true
})

const timeFormatter = new Intl.DateTimeFormat('en-US', {
    hour: "numeric",
    minute: "numeric",
    second: "numeric"
})

socket.onclose = function(event) {
    console.log(event);
};

socket.onmessage = function(event) {
    var processList = JSON.parse(event.data);
    var table = document.getElementById('table');

    processList.sort((a, b) => (a.Cpu > b.Cpu ? -1 : 1))

    clearTable(table);

    for (var index in processList) {
        addRow(table, processList[index]);
    }
};

let clearTable = function(table) {

    var rows = table.getElementsByTagName('tr');
    var count = rows.length;
    for (var index = count - 1; index > 0; index--) {
        table.removeChild(rows[index]);
    }
}

let addRow = function(table, process) {
    var tr = document.createElement('tr');
    tr.appendChild(createCell(process.Pid));
    tr.appendChild(createCell(process.Name));
    tr.appendChild(createCell(formatterPercent.format(process.Cpu)));
    tr.appendChild(createCell(formatterNumber.format(process.Memory)));
    tr.appendChild(createCell(process.User));
    tr.appendChild(createCell(process.Time.split('.')[0]));
    table.appendChild(tr);
}

let createCell = function(value) {
    var td = document.createElement('td');
    td.appendChild(document.createTextNode(value));
    return td;
}