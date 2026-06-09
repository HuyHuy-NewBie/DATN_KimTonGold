import 'package:flutter/material.dart';

import '../app_state.dart';
import '../formatters.dart';
import '../models.dart';
import '../widgets.dart';

class ProductsScreen extends StatefulWidget {
  const ProductsScreen({super.key, required this.state});

  final AppState state;

  @override
  State<ProductsScreen> createState() => _ProductsScreenState();
}

class _ProductsScreenState extends State<ProductsScreen> {
  final _searchController = TextEditingController();
  List<Product> products = const [];
  bool loading = true;
  String? error;
  String? line;
  String? status;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() {
      loading = true;
      error = null;
    });
    try {
      final data = await widget.state.api.products(
        search: _searchController.text.trim(),
        line: line,
        status: status,
      );
      setState(() => products = data);
    } catch (exception) {
      setState(() => error = exception.toString());
    } finally {
      if (mounted) {
        setState(() => loading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final canWrite = widget.state.user?.can('products:write') == true;
    return Scaffold(
      backgroundColor: Colors.transparent,
      floatingActionButton: canWrite
          ? FloatingActionButton.extended(
              onPressed: () => _openForm(),
              icon: const Icon(Icons.add),
              label: const Text('Thêm'),
            )
          : null,
      body: RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 88),
          children: [
            TextField(
              controller: _searchController,
              decoration: InputDecoration(
                labelText: 'Tìm sản phẩm',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: IconButton(
                  tooltip: 'Tìm',
                  onPressed: _load,
                  icon: const Icon(Icons.arrow_forward),
                ),
              ),
              onSubmitted: (_) => _load(),
            ),
            const SizedBox(height: 10),
            Row(
              children: [
                Expanded(
                  child: DropdownButtonFormField<String?>(
                    initialValue: line,
                    items: const [
                      DropdownMenuItem(value: null, child: Text('Tất cả dòng')),
                      DropdownMenuItem(value: 'Gold', child: Text('Vàng')),
                      DropdownMenuItem(value: 'Silver', child: Text('Bạc')),
                      DropdownMenuItem(
                        value: 'Diamond',
                        child: Text('Kim cương'),
                      ),
                    ],
                    onChanged: (value) {
                      setState(() => line = value);
                      _load();
                    },
                    decoration: const InputDecoration(labelText: 'Dòng'),
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: DropdownButtonFormField<String?>(
                    initialValue: status,
                    items: [
                      const DropdownMenuItem(
                        value: null,
                        child: Text('Tất cả trạng thái'),
                      ),
                      ...widget.state.metadata.productStatuses.map(
                        (item) =>
                            DropdownMenuItem(value: item, child: Text(item)),
                      ),
                    ],
                    onChanged: (value) {
                      setState(() => status = value);
                      _load();
                    },
                    decoration: const InputDecoration(labelText: 'Trạng thái'),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            if (loading)
              const SizedBox(height: 360, child: LoadingPane())
            else if (error != null)
              EmptyState(
                icon: Icons.error_outline,
                title: 'Không tải được sản phẩm',
                message: error,
              )
            else if (products.isEmpty)
              const EmptyState(
                icon: Icons.inventory_2_outlined,
                title: 'Không có sản phẩm',
              )
            else
              ...products.map(
                (product) => _ProductTile(
                  product: product,
                  canWrite: canWrite,
                  onEdit: () => _openForm(product),
                  onDelete: () => _delete(product),
                ),
              ),
          ],
        ),
      ),
    );
  }

  Future<void> _openForm([Product? product]) async {
    final saved = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      showDragHandle: true,
      builder: (context) =>
          ProductFormSheet(state: widget.state, product: product),
    );
    if (saved == true) {
      await _load();
    }
  }

  Future<void> _delete(Product product) async {
    final confirmed = await confirmAction(
      context,
      title: 'Xóa sản phẩm',
      message: 'Bạn muốn xóa ${product.name}?',
      confirmLabel: 'Xóa',
    );
    if (!confirmed) {
      return;
    }
    try {
      await widget.state.api.deleteProduct(product.id);
      if (mounted) {
        showSnack(context, 'Đã xóa sản phẩm');
      }
      await _load();
    } catch (exception) {
      if (mounted) {
        showSnack(context, exception.toString());
      }
    }
  }
}

class _ProductTile extends StatelessWidget {
  const _ProductTile({
    required this.product,
    required this.canWrite,
    required this.onEdit,
    required this.onDelete,
  });

  final Product product;
  final bool canWrite;
  final VoidCallback onEdit;
  final VoidCallback onDelete;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: SizedBox(
                width: 74,
                height: 74,
                child: product.images.isEmpty
                    ? ColoredBox(
                        color: Theme.of(
                          context,
                        ).colorScheme.surfaceContainerHighest,
                        child: const Icon(Icons.diamond_outlined),
                      )
                    : Image.network(
                        product.images.first,
                        fit: BoxFit.cover,
                        errorBuilder: (_, _, _) => const ColoredBox(
                          color: Color(0xFFEDE7D8),
                          child: Icon(Icons.diamond_outlined),
                        ),
                      ),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Expanded(
                        child: Text(
                          product.name,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                      ),
                      StatusChip(product.status),
                    ],
                  ),
                  Text(
                    '${product.category} • ${lineLabel(product.productLine)} • ${product.branchName ?? '--'}',
                  ),
                  const SizedBox(height: 6),
                  Text(
                    '${money(product.sellPrice)} • ${numberText(product.weight)}g',
                    style: Theme.of(context).textTheme.titleSmall,
                  ),
                  if (canWrite)
                    Wrap(
                      spacing: 6,
                      children: [
                        TextButton.icon(
                          onPressed: onEdit,
                          icon: const Icon(Icons.edit_outlined),
                          label: const Text('Sửa'),
                        ),
                        TextButton.icon(
                          onPressed: onDelete,
                          icon: const Icon(Icons.delete_outline),
                          label: const Text('Xóa'),
                        ),
                      ],
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class ProductFormSheet extends StatefulWidget {
  const ProductFormSheet({super.key, required this.state, this.product});

  final AppState state;
  final Product? product;

  @override
  State<ProductFormSheet> createState() => _ProductFormSheetState();
}

class _ProductFormSheetState extends State<ProductFormSheet> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController name;
  late final TextEditingController weight;
  late final TextEditingController processingFee;
  late final TextEditingController sellPrice;
  late final TextEditingController buyPrice;
  late final TextEditingController images;
  late final TextEditingController description;
  late final TextEditingController diamondCarat;
  late final TextEditingController diamondSize;
  String productLine = 'Gold';
  String catalogMode = 'Single';
  Set<String> assigned = {'Gold'};
  String status = 'Còn hàng';
  String category = '';
  String goldType = '';
  int? branchId;
  String? diamondShape;
  String? diamondCut;
  String? diamondColor;
  String? diamondClarity;
  String? diamondCertificate;
  bool saving = false;

  @override
  void initState() {
    super.initState();
    final product = widget.product;
    productLine = product?.productLine ?? 'Gold';
    catalogMode = product?.catalogMode ?? 'Single';
    assigned =
        (product?.assignedProductLines.isNotEmpty == true
                ? product!.assignedProductLines
                : [productLine])
            .toSet();
    status =
        product?.status ??
        widget.state.metadata.productStatuses.firstOrNull ??
        'Còn hàng';
    category =
        product?.category ?? _categoriesFor(productLine).firstOrNull ?? '';
    goldType =
        product?.goldType ?? _materialsFor(productLine).firstOrNull ?? '';
    branchId =
        product?.branchId ??
        widget.state.user?.branchId ??
        widget.state.branches.firstOrNull?.id;
    diamondShape = product?.diamondShape;
    diamondCut = product?.diamondCut;
    diamondColor = product?.diamondColor;
    diamondClarity = product?.diamondClarity;
    diamondCertificate = product?.diamondCertificate;
    name = TextEditingController(text: product?.name ?? '');
    weight = TextEditingController(text: (product?.weight ?? 0).toString());
    processingFee = TextEditingController(
      text: formatMoneyInput(product?.processingFee ?? 0),
    );
    sellPrice = TextEditingController(
      text: formatMoneyInput(product?.sellPrice ?? 0),
    );
    buyPrice = TextEditingController(
      text: formatMoneyInput(product?.buyPrice ?? 0),
    );
    images = TextEditingController(text: product?.images.join('; ') ?? '');
    description = TextEditingController(text: product?.description ?? '');
    diamondCarat = TextEditingController(
      text: product?.diamondCarat?.toString() ?? '',
    );
    diamondSize = TextEditingController(
      text: product?.diamondSize?.toString() ?? '',
    );
  }

  @override
  void dispose() {
    name.dispose();
    weight.dispose();
    processingFee.dispose();
    sellPrice.dispose();
    buyPrice.dispose();
    images.dispose();
    description.dispose();
    diamondCarat.dispose();
    diamondSize.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.viewInsetsOf(context).bottom;
    final hasDiamond = assigned.contains('Diamond');
    return Padding(
      padding: EdgeInsets.only(bottom: bottom),
      child: SafeArea(
        top: false,
        child: Form(
          key: _formKey,
          child: ListView(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 18),
            shrinkWrap: true,
            children: [
              Text(
                widget.product == null ? 'Thêm sản phẩm' : 'Sửa sản phẩm',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: 14),
              TextFormField(
                controller: name,
                decoration: const InputDecoration(labelText: 'Tên sản phẩm'),
                validator: _required,
              ),
              const SizedBox(height: 10),
              Row(
                children: [
                  Expanded(
                    child: DropdownButtonFormField<String>(
                      initialValue: productLine,
                      decoration: const InputDecoration(
                        labelText: 'Dòng chính',
                      ),
                      items: const [
                        DropdownMenuItem(value: 'Gold', child: Text('Vàng')),
                        DropdownMenuItem(value: 'Silver', child: Text('Bạc')),
                        DropdownMenuItem(
                          value: 'Diamond',
                          child: Text('Kim cương'),
                        ),
                      ],
                      onChanged: (value) {
                        if (value == null) return;
                        setState(() {
                          productLine = value;
                          if (catalogMode == 'Single') {
                            assigned = {value};
                          } else {
                            assigned.add(value);
                            if (assigned.length > 2) {
                              assigned = {
                                value,
                                assigned.firstWhere((item) => item != value),
                              };
                            }
                          }
                          category =
                              _categoriesFor(productLine).firstOrNull ??
                              category;
                          goldType =
                              _materialsFor(productLine).firstOrNull ??
                              goldType;
                        });
                      },
                    ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: DropdownButtonFormField<String>(
                      initialValue: catalogMode,
                      decoration: const InputDecoration(
                        labelText: 'Kiểu danh mục',
                      ),
                      items: const [
                        DropdownMenuItem(value: 'Single', child: Text('Đơn')),
                        DropdownMenuItem(
                          value: 'Multi',
                          child: Text('Kết hợp'),
                        ),
                      ],
                      onChanged: (value) {
                        if (value == null) return;
                        setState(() {
                          catalogMode = value;
                          assigned = value == 'Single'
                              ? {productLine}
                              : {...assigned, productLine}.take(2).toSet();
                        });
                      },
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Wrap(
                spacing: 8,
                runSpacing: 4,
                children: ['Gold', 'Silver', 'Diamond'].map((line) {
                  final selected = assigned.contains(line);
                  return FilterChip(
                    label: Text(lineLabel(line)),
                    selected: selected,
                    onSelected: catalogMode == 'Single'
                        ? null
                        : (value) {
                            setState(() {
                              if (value) {
                                assigned.add(line);
                              } else if (line != productLine) {
                                assigned.remove(line);
                              }
                              assigned.add(productLine);
                              if (assigned.length > 2) {
                                assigned.remove(
                                  assigned.firstWhere(
                                    (item) =>
                                        item != productLine && item != line,
                                  ),
                                );
                              }
                            });
                          },
                  );
                }).toList(),
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<String>(
                key: ValueKey('category-$productLine-$category'),
                initialValue: _dropdownValue(
                  category,
                  _categoriesFor(productLine),
                ),
                decoration: const InputDecoration(labelText: 'Danh mục'),
                items: _withCurrent(_categoriesFor(productLine), category)
                    .map(
                      (item) =>
                          DropdownMenuItem(value: item, child: Text(item)),
                    )
                    .toList(),
                onChanged: (value) =>
                    setState(() => category = value ?? category),
                validator: (_) =>
                    category.trim().isEmpty ? 'Vui lòng chọn danh mục' : null,
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<String>(
                key: ValueKey('material-$productLine-$goldType'),
                initialValue: _dropdownValue(
                  goldType,
                  _materialsFor(productLine),
                ),
                decoration: const InputDecoration(labelText: 'Chất liệu'),
                items: _withCurrent(_materialsFor(productLine), goldType)
                    .map(
                      (item) =>
                          DropdownMenuItem(value: item, child: Text(item)),
                    )
                    .toList(),
                onChanged: (value) =>
                    setState(() => goldType = value ?? goldType),
                validator: (_) =>
                    goldType.trim().isEmpty ? 'Vui lòng chọn chất liệu' : null,
              ),
              const SizedBox(height: 10),
              BranchDropdown(
                branches: _branchesWithCurrent(),
                value: branchId,
                includeAll: false,
                onChanged: (value) => setState(() => branchId = value),
              ),
              const SizedBox(height: 10),
              Row(
                children: [
                  Expanded(child: _numberField(weight, 'Khối lượng (g)')),
                  const SizedBox(width: 10),
                  Expanded(child: _numberField(processingFee, 'Phí gia công', isMoney: true)),
                ],
              ),
              const SizedBox(height: 10),
              Row(
                children: [
                  Expanded(child: _numberField(sellPrice, 'Giá bán', isMoney: true)),
                  const SizedBox(width: 10),
                  Expanded(child: _numberField(buyPrice, 'Giá mua', isMoney: true)),
                ],
              ),
              const SizedBox(height: 10),
              DropdownButtonFormField<String>(
                initialValue: _dropdownValue(
                  status,
                  widget.state.metadata.productStatuses,
                ),
                decoration: const InputDecoration(labelText: 'Trạng thái'),
                items:
                    _withCurrent(widget.state.metadata.productStatuses, status)
                        .map(
                          (item) =>
                              DropdownMenuItem(value: item, child: Text(item)),
                        )
                        .toList(),
                onChanged: (value) => setState(() => status = value ?? status),
              ),
              const SizedBox(height: 10),
              if (_splitImages(images.text).isNotEmpty)
                Padding(
                  padding: const EdgeInsets.only(bottom: 10),
                  child: SizedBox(
                    height: 100,
                    child: ListView(
                      scrollDirection: Axis.horizontal,
                      children: _splitImages(images.text).map((url) {
                        return Padding(
                          padding: const EdgeInsets.only(right: 8),
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(8),
                            child: Image.network(
                              url,
                              width: 100,
                              height: 100,
                              fit: BoxFit.cover,
                              errorBuilder: (_, _, _) => Container(
                                width: 100,
                                color: Colors.grey.shade200,
                                child: const Icon(Icons.broken_image),
                              ),
                            ),
                          ),
                        );
                      }).toList(),
                    ),
                  ),
                ),
              TextFormField(
                controller: images,
                onChanged: (_) => setState(() {}),
                decoration: const InputDecoration(
                  labelText: 'Ảnh sản phẩm',
                  hintText: 'Dán URL, phân tách bằng ; hoặc xuống dòng',
                ),
                minLines: 1,
                maxLines: 3,
              ),
              const SizedBox(height: 10),
              TextFormField(
                controller: description,
                decoration: const InputDecoration(labelText: 'Mô tả'),
                minLines: 2,
                maxLines: 4,
              ),
              if (hasDiamond) ...[
                const SectionTitle(title: 'Thông tin kim cương'),
                DropdownButtonFormField<String>(
                  initialValue: _dropdownValue(
                    diamondShape,
                    widget.state.metadata.diamondShapes,
                  ),
                  decoration: const InputDecoration(labelText: 'Hình dạng'),
                  items: widget.state.metadata.diamondShapes
                      .map(
                        (item) =>
                            DropdownMenuItem(value: item, child: Text(item)),
                      )
                      .toList(),
                  onChanged: (value) => setState(() => diamondShape = value),
                  validator: (_) =>
                      hasDiamond &&
                          (diamondShape == null || diamondShape!.isEmpty)
                      ? 'Vui lòng chọn hình dạng'
                      : null,
                ),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: _optionalDropdown(
                        'Giác cắt',
                        diamondCut,
                        widget.state.metadata.diamondCuts,
                        (value) => setState(() => diamondCut = value),
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: _optionalDropdown(
                        'Màu',
                        diamondColor,
                        widget.state.metadata.diamondColors,
                        (value) => setState(() => diamondColor = value),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: _optionalDropdown(
                        'Độ sạch',
                        diamondClarity,
                        widget.state.metadata.diamondClarities,
                        (value) => setState(() => diamondClarity = value),
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: _optionalDropdown(
                        'Chứng nhận',
                        diamondCertificate,
                        widget.state.metadata.diamondCertificates,
                        (value) => setState(() => diamondCertificate = value),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: _numberField(
                        diamondCarat,
                        'Carat',
                        required: false,
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: _numberField(
                        diamondSize,
                        'Kích thước ly',
                        required: false,
                      ),
                    ),
                  ],
                ),
              ],
              const SizedBox(height: 18),
              FilledButton.icon(
                onPressed: saving ? null : _save,
                icon: saving
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.save_outlined),
                label: const Text('Lưu sản phẩm'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  TextFormField _numberField(
    TextEditingController controller,
    String label, {
    bool required = true,
    bool isMoney = false,
  }) {
    return TextFormField(
      controller: controller,
      decoration: InputDecoration(labelText: label),
      keyboardType: const TextInputType.numberWithOptions(decimal: true),
      inputFormatters: isMoney ? [CurrencyInputFormatter()] : null,
      validator: (value) {
        if (!required && (value == null || value.trim().isEmpty)) {
          return null;
        }
        final number = _toDouble(value);
        if (number == null || number < 0) {
          return 'Không hợp lệ';
        }
        return null;
      },
    );
  }

  Widget _optionalDropdown(
    String label,
    String? value,
    List<String> options,
    ValueChanged<String?> onChanged,
  ) {
    return DropdownButtonFormField<String>(
      initialValue: _dropdownValue(value, options),
      decoration: InputDecoration(
        labelText: label,
        suffixIcon: value == null
            ? null
            : IconButton(
                tooltip: 'Bỏ chọn',
                onPressed: () => onChanged(null),
                icon: const Icon(Icons.clear),
              ),
      ),
      hint: const Text('Không chọn'),
      items: options
          .map((item) => DropdownMenuItem(value: item, child: Text(item)))
          .toList(),
      onChanged: onChanged,
    );
  }

  String? _dropdownValue(String? value, List<String> options) {
    if (value == null || value.isEmpty) {
      return null;
    }
    return options.contains(value) ? value : value;
  }

  List<String> _withCurrent(List<String> options, String? current) {
    if (current == null || current.isEmpty || options.contains(current)) {
      return options;
    }
    return [current, ...options];
  }

  List<Branch> _branchesWithCurrent() {
    if (branchId == null ||
        widget.state.branches.any((branch) => branch.id == branchId)) {
      return widget.state.branches;
    }
    return [
      Branch(
        id: branchId!,
        branchName: 'Chi nhánh #$branchId',
        address: null,
        phoneNumber: null,
        isActive: true,
      ),
      ...widget.state.branches,
    ];
  }

  List<String> _categoriesFor(String line) {
    return switch (line) {
      'Silver' => widget.state.metadata.silverCategories,
      'Diamond' => widget.state.metadata.diamondCategories,
      _ => widget.state.metadata.goldCategories,
    };
  }

  List<String> _materialsFor(String line) {
    return switch (line) {
      'Silver' => widget.state.metadata.silverMaterials,
      'Diamond' => widget.state.metadata.diamondMaterials,
      _ => widget.state.metadata.goldMaterials,
    };
  }

  String? _required(String? value) {
    return value == null || value.trim().isEmpty ? 'Không được để trống' : null;
  }

  double? _toDouble(String? value) {
    if (value == null) return null;
    final clean = value.trim().replaceAll(',', '');
    return double.tryParse(clean);
  }

  List<String> _splitImages(String value) {
    return value
        .split(RegExp(r'[;\n,]+'))
        .map((item) => item.trim())
        .where((item) => item.isNotEmpty)
        .take(3)
        .toList();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    if (branchId == null) {
      showSnack(context, 'Vui lòng chọn chi nhánh');
      return;
    }
    if (catalogMode == 'Multi' && assigned.length != 2) {
      showSnack(context, 'Sản phẩm kết hợp cần đúng 2 dòng danh mục');
      return;
    }
    if (assigned.contains('Diamond') &&
        diamondCarat.text.trim().isEmpty &&
        diamondSize.text.trim().isEmpty) {
      showSnack(
        context,
        'Sản phẩm kim cương cần nhập carat hoặc kích thước ly',
      );
      return;
    }

    final payload = ProductPayload(
      name: name.text.trim(),
      category: category,
      goldType: goldType,
      productLine: productLine,
      catalogMode: catalogMode,
      assignedProductLines: assigned.toList(),
      weight: _toDouble(weight.text) ?? 0,
      processingFee: _toDouble(processingFee.text) ?? 0,
      sellPrice: _toDouble(sellPrice.text) ?? 0,
      buyPrice: _toDouble(buyPrice.text) ?? 0,
      branchId: branchId!,
      images: _splitImages(images.text),
      description: description.text.trim().isEmpty
          ? null
          : description.text.trim(),
      status: status,
      diamondShape: assigned.contains('Diamond') ? diamondShape : null,
      diamondCut: assigned.contains('Diamond') ? diamondCut : null,
      diamondColor: assigned.contains('Diamond') ? diamondColor : null,
      diamondClarity: assigned.contains('Diamond') ? diamondClarity : null,
      diamondCarat: assigned.contains('Diamond')
          ? _toDouble(diamondCarat.text)
          : null,
      diamondSize: assigned.contains('Diamond')
          ? _toDouble(diamondSize.text)
          : null,
      diamondCertificate: assigned.contains('Diamond')
          ? diamondCertificate
          : null,
    );

    setState(() => saving = true);
    try {
      if (widget.product == null) {
        await widget.state.api.createProduct(payload);
      } else {
        await widget.state.api.updateProduct(widget.product!.id, payload);
      }
      if (mounted) {
        showSnack(context, 'Đã lưu sản phẩm');
        Navigator.pop(context, true);
      }
    } catch (exception) {
      if (mounted) {
        showSnack(context, exception.toString());
      }
    } finally {
      if (mounted) {
        setState(() => saving = false);
      }
    }
  }
}
