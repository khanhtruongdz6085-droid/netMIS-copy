package netcafe.dao;

import netcafe.model.Checks;
import netcafe.model.CustomerAccount;
import java.util.List;
import java.util.Optional;

public final class CustomerAccountDAO extends ListDAO<CustomerAccount> {
    public CustomerAccountDAO(List<CustomerAccount> accounts) { super(accounts); }

    public Optional<CustomerAccount> findByUsername(String username) {
        String key = Checks.name(username);
        return values.stream().filter(a -> a.getUsername().equalsIgnoreCase(key)).findFirst();
    }
}
