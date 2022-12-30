JPanel descriptionPanel = new JPanel();
descriptionPanel.setLayout(new GridBagLayout());
getContentPane().add(
  descriptionPanel,
  new GridBagConstraints(0, 0, 2, 1, 1.0, 0.0, GridBagConstraints.CENTER, GridBagConstraints.BOTH, new Insets(0, 0, 0,
    0), 0, 0));
descriptionPanel.setBackground(Color.white);
descriptionPanel.setBorder(BorderFactory.createMatteBorder(0, 0, 1, 0, Color.black));
  descriptionPanel.add(descriptionText, new GridBagConstraints(0, 0, 1, 1, 1.0, 0.0, GridBagConstraints.CENTER,
    GridBagConstraints.BOTH, new Insets(5, 5, 5, 5), 0, 0));
  descriptionText.setWrapStyleWord(true);
JPanel panel = new JPanel();
getContentPane().add(
  panel,
  new GridBagConstraints(0, 1, 1, 1, 1.0, 1.0, GridBagConstraints.CENTER, GridBagConstraints.NONE, new Insets(5, 5, 0,
    5), 0, 0));
panel.add(new JLabel(name + ":"));
panel.add(component);
JPanel buttonPanel = new JPanel();
getContentPane().add(
  buttonPanel,
  new GridBagConstraints(0, 2, 2, 1, 0.0, 0.0, GridBagConstraints.EAST, GridBagConstraints.NONE,
    new Insets(0, 0, 0, 0), 0, 0));
  JButton okButton = new JButton("OK");
  buttonPanel.add(okButton);
