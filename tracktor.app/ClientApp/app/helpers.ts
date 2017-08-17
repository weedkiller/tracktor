export var padNumber = function (n: number) {
    var s = n.toString();
    if (s.length === 1) {
        return "0" + s;
    }
    return s;
};

export var isZero = function (n: number) {
    return (n === 0);
};