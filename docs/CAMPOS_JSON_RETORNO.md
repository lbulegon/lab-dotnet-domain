# 📋 Campos no JSON de Retorno vs Campos Exibidos

## Campos Retornados no JSON

O `DomainController` retorna a entidade `Domain` completa, que possui os seguintes campos:

```json
{
  "id": 1,
  "name": "umbler.com",
  "ip": "187.84.237.146",
  "updatedAt": "2025-12-17T22:18:00",
  "whoIs": "... (texto muito longo do WHOIS raw) ...",
  "ttl": 3600,
  "hostedAt": "RedeHost Internet Ltda."
}
```

---

## Campos Exibidos no Frontend

Atualmente, estamos exibindo apenas **3 de 7 campos**:

### ✅ **Exibidos:**

1. **`name`** - Nome do domínio
   - Exibido no header do card

2. **`ip`** - Endereço IP
   - Exibido em box dedicado com ícone

3. **`hostedAt`** - Empresa hospedadora
   - Exibido em box dedicado com ícone

### ❌ **NÃO Exibidos (mas presentes no JSON):**

4. **`id`** - ID interno do banco
   - Não deve ser exibido (informação técnica/interna)

5. **`updatedAt`** - Data da última atualização
   - Não exibido (informação técnica)
   - Poderia ser útil mostrar "Atualizado em..."

6. **`whoIs`** - Dados brutos do WHOIS
   - Não exibido (texto muito grande, dados técnicos)
   - Pode ter centenas de linhas

7. **`ttl`** - Time To Live
   - Não exibido (informação técnica)
   - Poderia ser útil mostrar "Cache válido por X horas"

---

## 📝 Informações que DEVERIAM ser exibidas (segundo README)

Segundo o README, o retorno esperado deveria incluir:

- ✅ **Name servers** (ns254.umbler.com) - **NÃO ESTÁ SENDO EXIBIDO**
- ✅ **IP do registro A** - Exibido ✅
- ✅ **Empresa que está hospedado** - Exibido ✅

**Observação:** Name Servers não estão sendo extraídos nem salvos no banco atualmente.

---

## 🎯 Recomendações

### Campos que Poderiam Ser Adicionados à Exibição:

1. **Name Servers** (NS records)
   - Extrair do DNS ou WHOIS
   - Adicionar ao modelo Domain (ou criar campo separado)
   - Exibir como lista formatada

2. **Data de Atualização** (updatedAt)
   - Formatar como "Atualizado há X minutos/horas"
   - Útil para o usuário saber se os dados estão frescos

3. **TTL Formatado** (opcional)
   - Mostrar "Cache válido por X horas/minutos"
   - Ajuda o usuário a entender quando os dados serão atualizados

### Campos que NÃO devem ser exibidos:

- **`id`** - Informação interna
- **`whoIs` raw** - Muito grande, dados técnicos brutos

---

## 💡 Próximos Passos Sugeridos

1. Extrair e exibir Name Servers do DNS
2. Adicionar campo "Atualizado em" formatado
3. Considerar usar DomainViewModel ao invés da entidade Domain (já criado, mas não está sendo usado)

